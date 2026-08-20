using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.WaitingList;

public static class WaitingListApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound("SchedulingCapacity.WaitingList.NotFound", "errors.schedulingCapacity.waitingList.notFound");
    public static readonly Error SlotAlreadyHeld = Error.Conflict("SchedulingCapacity.WaitingList.SlotAlreadyHeld", "errors.schedulingCapacity.waitingList.slotAlreadyHeld");
}

public sealed class CreateWaitingListEntryCommandHandler(
    IWaitingListEntryRepository repository,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<CreateWaitingListEntryCommand, DriveOS.SharedKernel.Identifiers.WaitingListEntryId>
{
    public async Task<Result<DriveOS.SharedKernel.Identifiers.WaitingListEntryId>> Handle(CreateWaitingListEntryCommand command, CancellationToken cancellationToken)
    {
        Result<WaitingListPriorityResult> priority = WaitingListPriorityPolicy.Calculate(command.Priority, DateTimeOffset.UtcNow);
        if (priority.IsFailure) return Result.Failure<DriveOS.SharedKernel.Identifiers.WaitingListEntryId>(priority.Error);
        var id = DriveOS.SharedKernel.Identifiers.WaitingListEntryId.New();
        Result<WaitingListEntry> created = WaitingListEntry.Create(id, command.OrganizationId, command.StudentId, command.RequestedSessionType,
            command.PreferredFromUtc, command.PreferredToUtc, command.DurationMinutes, command.PreferredBranchId, command.PreferredInstructorId,
            priority.Value.BaseScore, priority.Value.Explanation, command.Reason, command.ExpiresAtUtc);
        if (created.IsFailure) return Result.Failure<DriveOS.SharedKernel.Identifiers.WaitingListEntryId>(created.Error);
        repository.Add(created.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(id);
    }
}

public abstract class WaitingListMutationHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
{
    protected async Task<Result> Mutate(DriveOS.SharedKernel.Identifiers.OrganizationId organizationId, DriveOS.SharedKernel.Identifiers.WaitingListEntryId entryId,
        Func<WaitingListEntry, Result> mutation, CancellationToken cancellationToken)
    {
        WaitingListEntry? entry = await repository.GetByIdForUpdateAsync(entryId, organizationId, cancellationToken);
        if (entry is null) return Result.Failure(WaitingListApplicationErrors.NotFound);
        entry.ExpireIfNeeded(DateTimeOffset.UtcNow);
        Result result = mutation(entry);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateWaitingListPreferencesCommandHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
    : WaitingListMutationHandler(repository, unitOfWork), ICommandHandler<UpdateWaitingListPreferencesCommand>
{
    public Task<Result> Handle(UpdateWaitingListPreferencesCommand command, CancellationToken cancellationToken) =>
        Mutate(command.OrganizationId, command.EntryId, x => x.UpdatePreferences(command.PreferredFromUtc, command.PreferredToUtc, command.PreferredBranchId, command.PreferredInstructorId, command.ExpiresAtUtc), cancellationToken);
}

public sealed class ProposeWaitingListSlotCommandHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
    : ICommandHandler<ProposeWaitingListSlotCommand, DriveOS.SharedKernel.Identifiers.WaitingListProposalId>
{
    public async Task<Result<DriveOS.SharedKernel.Identifiers.WaitingListProposalId>> Handle(ProposeWaitingListSlotCommand command, CancellationToken cancellationToken)
    {
        WaitingListEntry? entry = await repository.GetByIdForUpdateAsync(command.EntryId, command.OrganizationId, cancellationToken);
        if (entry is null) return Result.Failure<DriveOS.SharedKernel.Identifiers.WaitingListProposalId>(WaitingListApplicationErrors.NotFound);
        entry.ExpireIfNeeded(DateTimeOffset.UtcNow);
        Result<DriveOS.SharedKernel.Identifiers.WaitingListProposalId> result = entry.Propose(command.StartAtUtc, command.EndAtUtc, command.BranchId, command.InstructorId, command.ExpiresAtUtc);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return result;
    }
}

public sealed class HoldWaitingListProposalCommandHandler(
    IWaitingListEntryRepository repository,
    IWaitingListSlotLock slotLock,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<HoldWaitingListProposalCommand>
{
    public async Task<Result> Handle(HoldWaitingListProposalCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            WaitingListEntry? entry = await repository.GetByIdForUpdateAsync(command.EntryId, command.OrganizationId, cancellationToken);
            if (entry is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(WaitingListApplicationErrors.NotFound);
            }
            entry.ExpireIfNeeded(DateTimeOffset.UtcNow);
            WaitingListProposal? proposal = entry.Proposals.SingleOrDefault(x => x.Id == command.ProposalId);
            if (proposal is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(WaitingListErrors.ProposalNotFound);
            }

            await slotLock.AcquireAsync(command.OrganizationId, proposal.SlotKey, cancellationToken);
            if (await repository.HasActiveHoldAsync(command.OrganizationId, proposal.SlotKey, command.EntryId, cancellationToken))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(WaitingListApplicationErrors.SlotAlreadyHeld);
            }

            Result result = entry.Hold(command.ProposalId, command.HeldUntilUtc);
            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return result;
            }
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class AcceptWaitingListProposalCommandHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
    : WaitingListMutationHandler(repository, unitOfWork), ICommandHandler<AcceptWaitingListProposalCommand>
{
    public Task<Result> Handle(AcceptWaitingListProposalCommand command, CancellationToken cancellationToken) =>
        Mutate(command.OrganizationId, command.EntryId, x => x.Accept(command.ProposalId), cancellationToken);
}

public sealed class FulfillWaitingListEntryCommandHandler(
    IWaitingListEntryRepository repository,
    IBookingRepository bookingRepository,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<FulfillWaitingListEntryCommand>
{
    public async Task<Result> Handle(FulfillWaitingListEntryCommand command, CancellationToken cancellationToken)
    {
        WaitingListEntry? entry = await repository.GetByIdForUpdateAsync(command.EntryId, command.OrganizationId, cancellationToken);
        if (entry is null) return Result.Failure(WaitingListApplicationErrors.NotFound);
        entry.ExpireIfNeeded(DateTimeOffset.UtcNow);
        WaitingListProposal? proposal = entry.Proposals.SingleOrDefault(x => x.Id == command.ProposalId);
        if (proposal is null) return Result.Failure(WaitingListErrors.ProposalNotFound);

        Booking? booking = await bookingRepository.GetByIdAsync(command.BookingId, command.OrganizationId, cancellationToken);
        if (booking is null || booking.Status is not (BookingStatus.Reserved or BookingStatus.Confirmed) ||
            booking.StartAtUtc != proposal.StartAtUtc || booking.EndAtUtc != proposal.EndAtUtc ||
            (proposal.BranchId.HasValue && booking.BranchId != proposal.BranchId) ||
            !booking.Participants.Any(x => x.ParticipantType == BookingParticipantType.Student && x.ExternalParticipantId == entry.StudentId.Value))
            return Result.Failure(WaitingListErrors.FulfillmentMismatch);

        Result result = entry.Fulfill(command.ProposalId, command.BookingId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeclineWaitingListProposalCommandHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
    : WaitingListMutationHandler(repository, unitOfWork), ICommandHandler<DeclineWaitingListProposalCommand>
{
    public Task<Result> Handle(DeclineWaitingListProposalCommand command, CancellationToken cancellationToken) =>
        Mutate(command.OrganizationId, command.EntryId, x => x.Decline(command.ProposalId, command.Reason), cancellationToken);
}

public sealed class CancelWaitingListEntryCommandHandler(IWaitingListEntryRepository repository, ISchedulingCapacityUnitOfWork unitOfWork)
    : WaitingListMutationHandler(repository, unitOfWork), ICommandHandler<CancelWaitingListEntryCommand>
{
    public Task<Result> Handle(CancelWaitingListEntryCommand command, CancellationToken cancellationToken) =>
        Mutate(command.OrganizationId, command.EntryId, x => x.Cancel(command.Reason), cancellationToken);
}
