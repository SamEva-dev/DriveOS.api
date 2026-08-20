using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Conflicts;

public static class SchedulingConflictApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound("SchedulingCapacity.Conflict.NotFound", "errors.schedulingCapacity.conflict.notFound");
    public static readonly Error InvalidResolution = Error.Validation("SchedulingCapacity.Conflict.InvalidResolution", "errors.schedulingCapacity.conflict.invalidResolution");
    public static readonly Error AuthenticatedUserRequired = Error.Validation("SchedulingCapacity.Conflict.AuthenticatedUserRequired", "errors.schedulingCapacity.conflict.authenticatedUserRequired");
}

public sealed class RefreshSchedulingConflictsCommandHandler(
    DriveOS.Modules.SchedulingCapacity.Domain.Bookings.IBookingRepository bookingRepository,
    ISchedulingConflictInboxService service)
    : ICommandHandler<RefreshSchedulingConflictsCommand, SchedulingConflictScanResponse>
{
    public async Task<Result<SchedulingConflictScanResponse>> Handle(RefreshSchedulingConflictsCommand command, CancellationToken cancellationToken)
    {
        DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Booking? booking = await bookingRepository.GetByIdAsync(command.BookingId, command.OrganizationId, cancellationToken);
        if (booking is null) return Result.Failure<SchedulingConflictScanResponse>(SchedulingConflictApplicationErrors.NotFound);
        return Result.Success(await service.RefreshAsync(command.OrganizationId, command.BookingId, cancellationToken));
    }
}

public sealed class ResolveSchedulingConflictCommandHandler(
    ISchedulingConflictRepository repository,
    ICurrentUser currentUser,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<ResolveSchedulingConflictCommand>
{
    public async Task<Result> Handle(ResolveSchedulingConflictCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(SchedulingConflictResolution), command.Resolution)) return Result.Failure(SchedulingConflictApplicationErrors.InvalidResolution);
        SchedulingConflict? conflict = await repository.GetByIdForUpdateAsync(command.ConflictId, command.OrganizationId, cancellationToken);
        if (conflict is null) return Result.Failure(SchedulingConflictApplicationErrors.NotFound);
        Result result = conflict.Resolve((SchedulingConflictResolution)command.Resolution, command.Reason, currentUser.UserId);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class OverrideSchedulingConflictCommandHandler(
    ISchedulingConflictRepository repository,
    ICurrentUser currentUser,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<OverrideSchedulingConflictCommand>
{
    public async Task<Result> Handle(OverrideSchedulingConflictCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId) return Result.Failure(SchedulingConflictApplicationErrors.AuthenticatedUserRequired);
        SchedulingConflict? conflict = await repository.GetByIdForUpdateAsync(command.ConflictId, command.OrganizationId, cancellationToken);
        if (conflict is null) return Result.Failure(SchedulingConflictApplicationErrors.NotFound);
        Result result = conflict.Override(command.Reason, command.Risk, userId, command.ExpiresAtUtc);
        if (result.IsFailure) return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
