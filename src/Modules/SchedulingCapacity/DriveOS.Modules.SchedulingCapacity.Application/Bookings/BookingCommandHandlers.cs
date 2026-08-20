using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public static class BookingApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "SchedulingCapacity.Booking.NotFound",
        "errors.schedulingCapacity.booking.notFound");
}

internal static class BookingConflictMapping
{
    public static BookingConflictCheckResponse ToResponse(this BookingConflictAssessment assessment) => new(
        assessment.BookingId.Value,
        assessment.StartAtUtc,
        assessment.EndAtUtc,
        assessment.IsConflictFree,
        assessment.Conflicts.Select(x => new BookingConflictResponse(
            (int)x.Type,
            x.CalendarResourceId.Value,
            x.ConflictingBookingId?.Value,
            x.RequestedQuantity,
            x.AvailableCapacity,
            x.Reason)).ToArray());
}

public sealed class CreateBookingCommandHandler(
    IBookingRepository repository,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCreationIdempotencyLock idempotencyLock,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<CreateBookingCommand, BookingId>
{
    public async Task<Result<BookingId>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        string idempotencyKey = command.IdempotencyKey?.Trim() ?? string.Empty;
        if (idempotencyKey.Length is < 8 or > 120)
            return Result.Failure<BookingId>(BookingErrors.InvalidCreationIdempotency);
        if (!Enum.IsDefined(typeof(BookingType), command.BookingType))
            return Result.Failure<BookingId>(BookingErrors.InvalidType);
        if (!Enum.IsDefined(typeof(BookingNotificationPolicy), command.NotificationPolicy))
            return Result.Failure<BookingId>(BookingErrors.InvalidCreationDetails);

        string fingerprint = BookingCreationFingerprint.Create(command);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await idempotencyLock.AcquireAsync(command.OrganizationId, idempotencyKey, cancellationToken);

            Booking? existing = await repository.GetByCreationIdempotencyKeyAsync(command.OrganizationId, idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return string.Equals(existing.CreationRequestFingerprint, fingerprint, StringComparison.Ordinal)
                    ? Result.Success(existing.Id)
                    : Result.Failure<BookingId>(BookingErrors.CreationIdempotencyConflict);
            }

            Error? referenceError = await referenceValidationGateway.ValidateAsync(
                command.OrganizationId,
                command.BranchId,
                command.BookingType,
                command.TrainingCategory,
                command.Resources,
                command.Participants,
                cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingId>(referenceError);
            }

            BookingId id = BookingId.New();
            Result<Booking> created = Booking.Create(
                id,
                command.OrganizationId,
                command.BranchId,
                (BookingType)command.BookingType,
                command.StartAtUtc,
                command.EndAtUtc,
                command.Title,
                new BookingCreationDetails(
                    idempotencyKey,
                    fingerprint,
                    command.TrainingPathId,
                    command.TrainingCategory,
                    command.Objectives,
                    command.MeetingPoint,
                    command.PricingReference,
                    command.TrainingCreditAccountId,
                    command.CreditQuantity,
                    command.Notes,
                    (BookingNotificationPolicy)command.NotificationPolicy));

            if (created.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingId>(created.Error);
            }

            Booking booking = created.Value;

            foreach (CreateBookingResourceRequest resource in command.Resources)
            {
                Result<BookingResourceId> result = booking.AddResource(
                    BookingResourceId.New(),
                    new CalendarResourceId(resource.CalendarResourceId),
                    resource.Quantity);

                if (result.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingId>(result.Error);
                }
            }

            foreach (CreateBookingParticipantRequest participant in command.Participants)
            {
                if (!Enum.IsDefined(typeof(BookingParticipantType), participant.ParticipantType))
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingId>(BookingErrors.InvalidParticipant);
                }

                Result<BookingParticipantId> result = booking.AddParticipant(
                    BookingParticipantId.New(),
                    (BookingParticipantType)participant.ParticipantType,
                    participant.ExternalParticipantId);

                if (result.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingId>(result.Error);
                }
            }

            repository.Add(booking);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(id);
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

internal static class BookingCreationFingerprint
{
    internal static string Create(CreateBookingCommand command)
    {
        static string N(string? value) => value?.Trim() ?? string.Empty;
        string resources = string.Join(',', command.Resources
            .OrderBy(x => x.CalendarResourceId)
            .ThenBy(x => x.Quantity)
            .Select(x => $"{x.CalendarResourceId:N}:{x.Quantity}"));
        string participants = string.Join(',', command.Participants
            .OrderBy(x => x.ParticipantType)
            .ThenBy(x => x.ExternalParticipantId)
            .Select(x => $"{x.ParticipantType}:{x.ExternalParticipantId:N}"));

        string canonical = string.Join('|',
            command.OrganizationId.Value.ToString("N"),
            command.BranchId?.Value.ToString("N") ?? string.Empty,
            command.BookingType.ToString(),
            command.StartAtUtc.ToUniversalTime().ToString("O"),
            command.EndAtUtc.ToUniversalTime().ToString("O"),
            N(command.Title),
            command.TrainingPathId?.ToString("N") ?? string.Empty,
            N(command.TrainingCategory),
            N(command.Objectives),
            N(command.MeetingPoint),
            N(command.PricingReference),
            command.TrainingCreditAccountId?.ToString("N") ?? string.Empty,
            command.CreditQuantity?.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            N(command.Notes),
            command.NotificationPolicy.ToString(),
            resources,
            participants);

        byte[] hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed class CheckBookingConflictsCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService) : ICommandHandler<CheckBookingConflictsCommand, BookingConflictCheckResponse>
{
    public async Task<Result<BookingConflictCheckResponse>> Handle(CheckBookingConflictsCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await repository.GetByIdAsync(command.BookingId, command.OrganizationId, cancellationToken);
        if (booking is null)
            return Result.Failure<BookingConflictCheckResponse>(BookingApplicationErrors.NotFound);

        BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
        return Result.Success(assessment.ToResponse());
    }
}

public sealed class HoldBookingSlotCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<HoldBookingSlotCommand, BookingConflictCheckResponse>
{
    public async Task<Result<BookingConflictCheckResponse>> Handle(HoldBookingSlotCommand command, CancellationToken cancellationToken)
    {
        if (command.DurationMinutes is < 1 or > 15)
            return Result.Failure<BookingConflictCheckResponse>(BookingErrors.InvalidSlotHold);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(BookingApplicationErrors.NotFound);
            }

            await capacityLock.AcquireAsync(command.OrganizationId, booking.Resources.Select(x => x.CalendarResourceId).ToArray(), cancellationToken);

            Error? referenceError = await BookingReferenceRevalidation.ValidateAsync(booking, referenceValidationGateway, cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(referenceError);
            }

            BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
            if (!assessment.IsConflictFree)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(assessment.ToResponse());
            }

            DateTimeOffset now = clock.UtcNow;
            Result held = booking.Hold(assessment, now.AddMinutes(command.DurationMinutes), now);
            if (held.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(held.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(assessment.ToResponse());
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class ReserveBookingCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<ReserveBookingCommand, BookingConflictCheckResponse>
{
    public async Task<Result<BookingConflictCheckResponse>> Handle(ReserveBookingCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(BookingApplicationErrors.NotFound);
            }

            await capacityLock.AcquireAsync(
                command.OrganizationId,
                booking.Resources.Select(x => x.CalendarResourceId).ToArray(),
                cancellationToken);

            Error? referenceError = await BookingReferenceRevalidation.ValidateAsync(
                booking,
                referenceValidationGateway,
                cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(referenceError);
            }

            BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
            if (!assessment.IsConflictFree)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(assessment.ToResponse());
            }

            Result reserved = booking.Reserve(assessment);
            if (reserved.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(reserved.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(assessment.ToResponse());
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public abstract class BookingMutationHandler(
    IBookingRepository repository,
    ISchedulingCapacityUnitOfWork unitOfWork)
{
    protected async Task<Result> Mutate(
        OrganizationId organizationId,
        BookingId bookingId,
        Func<Booking, Result> mutation,
        CancellationToken cancellationToken)
    {
        Booking? booking = await repository.GetByIdForUpdateAsync(bookingId, organizationId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingApplicationErrors.NotFound);

        Result result = mutation(booking);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class ConfirmBookingCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCreditReservationGateway creditReservationGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork) : ICommandHandler<ConfirmBookingCommand, BookingConflictCheckResponse>
{
    public async Task<Result<BookingConflictCheckResponse>> Handle(ConfirmBookingCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(BookingApplicationErrors.NotFound);
            }

            if (booking.Status != BookingStatus.Reserved)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(BookingErrors.ConfirmationNotAllowed);
            }

            await capacityLock.AcquireAsync(
                command.OrganizationId,
                booking.Resources.Select(x => x.CalendarResourceId).ToArray(),
                cancellationToken);

            Error? referenceError = await BookingReferenceRevalidation.ValidateAsync(booking, referenceValidationGateway, cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(referenceError);
            }

            BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
            if (!assessment.IsConflictFree)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(assessment.ToResponse());
            }

            if (booking.CreditReservationStatus == BookingCreditReservationStatus.Pending)
            {
                if (!booking.TrainingCreditAccountId.HasValue || !booking.CreditQuantity.HasValue)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingConflictCheckResponse>(BookingErrors.CreditReservationRequired);
                }

                Result<BookingCreditReservationResult> creditReservation = await creditReservationGateway.ReserveAsync(
                    command.OrganizationId,
                    booking.TrainingCreditAccountId.Value,
                    booking.CreditQuantity.Value,
                    booking.Id,
                    command.ActorUserId,
                    cancellationToken);
                if (creditReservation.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingConflictCheckResponse>(creditReservation.Error);
                }

                Result marked = booking.MarkCreditReserved(creditReservation.Value.Reference);
                if (marked.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingConflictCheckResponse>(marked.Error);
                }
            }

            Result confirmed = booking.Confirm();
            if (confirmed.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingConflictCheckResponse>(confirmed.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(assessment.ToResponse());
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class PreviewRescheduleBookingCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<PreviewRescheduleBookingCommand, BookingRescheduleImpactResponse>
{
    public async Task<Result<BookingRescheduleImpactResponse>> Handle(PreviewRescheduleBookingCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(BookingApplicationErrors.NotFound);
            }

            BookingRescheduleHistory? existing = booking.RescheduleHistory.SingleOrDefault(x => x.OperationId == command.OperationId);
            if (existing is not null)
            {
                Result replayValidation = ValidateReplay(existing, command);
                if (replayValidation.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(replayValidation.Error);
                }

                BookingRescheduleImpactResponse replay = BookingRescheduleImpactFactory.CreateReplay(booking, existing);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(replay);
            }

            ProposedReschedule proposed = ProposedReschedule.From(booking, command.BranchId ?? booking.BranchId, command.Resources);
            await capacityLock.AcquireAsync(command.OrganizationId, proposed.AllLockedResourceIds, cancellationToken);

            Error? referenceError = await referenceValidationGateway.ValidateAsync(
                booking.OrganizationId,
                proposed.BranchId,
                (int)booking.BookingType,
                booking.TrainingCategory,
                proposed.Resources,
                booking.Participants.Select(x => new CreateBookingParticipantRequest((int)x.ParticipantType, x.ExternalParticipantId)).ToArray(),
                cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(referenceError);
            }

            DateTimeOffset previousStart = booking.StartAtUtc;
            DateTimeOffset previousEnd = booking.EndAtUtc;
            BranchId? previousBranch = booking.BranchId;

            Result rescheduled = booking.Reschedule(
                command.OperationId,
                command.StartAtUtc,
                command.EndAtUtc,
                proposed.BranchId,
                command.Reason,
                proposed.ResourcesChanged,
                proposed.NewResourceFingerprint,
                clock.UtcNow);
            if (rescheduled.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(rescheduled.Error);
            }

            if (proposed.ResourcesChanged)
            {
                Result replaced = booking.ReplaceResources(proposed.Replacements);
                if (replaced.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(replaced.Error);
                }
            }

            BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
            BookingRescheduleImpactResponse response = BookingRescheduleImpactFactory.Create(
                booking,
                command.OperationId,
                previousStart,
                previousEnd,
                previousBranch,
                proposed.ResourcesChanged,
                assessment,
                existing is not null);

            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Success(response);
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static Result ValidateReplay(BookingRescheduleHistory existing, PreviewRescheduleBookingCommand command)
    {
        string requestedFingerprint = command.Resources is null
            ? existing.NewResourceFingerprint
            : ProposedReschedule.Fingerprint(command.Resources);
        BranchId? requestedBranch = command.BranchId ?? existing.NewBranchId;
        bool same = existing.NewStartAtUtc == command.StartAtUtc.ToUniversalTime()
                    && existing.NewEndAtUtc == command.EndAtUtc.ToUniversalTime()
                    && existing.NewBranchId == requestedBranch
                    && string.Equals(existing.Reason, command.Reason?.Trim(), StringComparison.Ordinal)
                    && string.Equals(existing.NewResourceFingerprint, requestedFingerprint, StringComparison.Ordinal);
        return same ? Result.Success() : Result.Failure(BookingErrors.RescheduleOperationConflict);
    }
}

public sealed class RescheduleBookingCommandHandler(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService,
    IBookingReferenceValidationGateway referenceValidationGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RescheduleBookingCommand, BookingRescheduleImpactResponse>
{
    public async Task<Result<BookingRescheduleImpactResponse>> Handle(RescheduleBookingCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(BookingApplicationErrors.NotFound);
            }

            BookingRescheduleHistory? existing = booking.RescheduleHistory.SingleOrDefault(x => x.OperationId == command.OperationId);
            if (existing is not null)
            {
                string requestedFingerprint = command.Resources is null
                    ? existing.NewResourceFingerprint
                    : ProposedReschedule.Fingerprint(command.Resources);
                BranchId? requestedBranch = command.BranchId ?? existing.NewBranchId;
                bool same = existing.NewStartAtUtc == command.StartAtUtc.ToUniversalTime()
                            && existing.NewEndAtUtc == command.EndAtUtc.ToUniversalTime()
                            && existing.NewBranchId == requestedBranch
                            && string.Equals(existing.Reason, command.Reason?.Trim(), StringComparison.Ordinal)
                            && string.Equals(existing.NewResourceFingerprint, requestedFingerprint, StringComparison.Ordinal);
                if (!same)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(BookingErrors.RescheduleOperationConflict);
                }

                BookingRescheduleImpactResponse replayResponse = BookingRescheduleImpactFactory.CreateReplay(booking, existing);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(replayResponse);
            }

            ProposedReschedule proposed = ProposedReschedule.From(booking, command.BranchId ?? booking.BranchId, command.Resources);
            await capacityLock.AcquireAsync(command.OrganizationId, proposed.AllLockedResourceIds, cancellationToken);

            Error? referenceError = await referenceValidationGateway.ValidateAsync(
                booking.OrganizationId,
                proposed.BranchId,
                (int)booking.BookingType,
                booking.TrainingCategory,
                proposed.Resources,
                booking.Participants.Select(x => new CreateBookingParticipantRequest((int)x.ParticipantType, x.ExternalParticipantId)).ToArray(),
                cancellationToken);
            if (referenceError is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(referenceError);
            }

            DateTimeOffset previousStart = booking.StartAtUtc;
            DateTimeOffset previousEnd = booking.EndAtUtc;
            BranchId? previousBranch = booking.BranchId;
            BookingStatus previousStatus = booking.Status;

            Result rescheduled = booking.Reschedule(
                command.OperationId,
                command.StartAtUtc,
                command.EndAtUtc,
                proposed.BranchId,
                command.Reason,
                proposed.ResourcesChanged,
                proposed.NewResourceFingerprint,
                clock.UtcNow);
            if (rescheduled.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingRescheduleImpactResponse>(rescheduled.Error);
            }

            if (proposed.ResourcesChanged)
            {
                Result replaced = booking.ReplaceResources(proposed.Replacements);
                if (replaced.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(replaced.Error);
                }
            }

            BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
            BookingRescheduleImpactResponse response = BookingRescheduleImpactFactory.Create(
                booking,
                command.OperationId,
                previousStart,
                previousEnd,
                previousBranch,
                proposed.ResourcesChanged,
                assessment);

            if (!assessment.IsConflictFree)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(response);
            }

            if (previousStatus is BookingStatus.Reserved or BookingStatus.Confirmed)
            {
                Result reserved = booking.Reserve(assessment);
                if (reserved.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(reserved.Error);
                }
            }

            if (previousStatus == BookingStatus.Confirmed)
            {
                Result confirmed = booking.Confirm();
                if (confirmed.IsFailure)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result.Failure<BookingRescheduleImpactResponse>(confirmed.Error);
                }
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(response with { CanConfirm = true });
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

internal sealed record ProposedReschedule(
    BranchId? BranchId,
    IReadOnlyCollection<CreateBookingResourceRequest> Resources,
    IReadOnlyCollection<(BookingResourceId Id, CalendarResourceId ResourceId, int Quantity)> Replacements,
    IReadOnlyCollection<CalendarResourceId> AllLockedResourceIds,
    bool ResourcesChanged,
    string NewResourceFingerprint)
{
    internal static ProposedReschedule From(
        Booking booking,
        BranchId? branchId,
        IReadOnlyCollection<BookingRescheduleResourceRequest>? requestedResources)
    {
        CreateBookingResourceRequest[] resources = requestedResources is null
            ? booking.Resources.Select(x => new CreateBookingResourceRequest(x.CalendarResourceId.Value, x.Quantity)).ToArray()
            : requestedResources.Select(x => new CreateBookingResourceRequest(x.CalendarResourceId, x.Quantity)).ToArray();
        string oldFingerprint = Booking.ResourceFingerprint(booking.Resources);
        string newFingerprint = Fingerprint(resources.Select(x => new BookingRescheduleResourceRequest(x.CalendarResourceId, x.Quantity)));
        bool changed = !string.Equals(oldFingerprint, newFingerprint, StringComparison.Ordinal);
        var replacements = resources
            .Select(x => (BookingResourceId.New(), new CalendarResourceId(x.CalendarResourceId), x.Quantity))
            .ToArray();
        CalendarResourceId[] locked = booking.Resources.Select(x => x.CalendarResourceId)
            .Concat(resources.Select(x => new CalendarResourceId(x.CalendarResourceId)))
            .Distinct()
            .ToArray();
        return new ProposedReschedule(branchId, resources, replacements, locked, changed, newFingerprint);
    }

    internal static string Fingerprint(IEnumerable<BookingRescheduleResourceRequest> resources) =>
        string.Join("|", resources
            .OrderBy(x => x.CalendarResourceId)
            .Select(x => $"{x.CalendarResourceId:N}:{x.Quantity}"));
}

public static class BookingReferenceRevalidation
{
    public static Task<Error?> ValidateAsync(
        Booking booking,
        IBookingReferenceValidationGateway referenceValidationGateway,
        CancellationToken cancellationToken) =>
        referenceValidationGateway.ValidateAsync(
            booking.OrganizationId,
            booking.BranchId,
            (int)booking.BookingType,
            booking.TrainingCategory,
            booking.Resources.Select(x => new CreateBookingResourceRequest(x.CalendarResourceId.Value, x.Quantity)).ToArray(),
            booking.Participants.Select(x => new CreateBookingParticipantRequest((int)x.ParticipantType, x.ExternalParticipantId)).ToArray(),
            cancellationToken);
}

public sealed class PreviewCancelBookingCommandHandler(
    IBookingRepository repository,
    IBookingCancellationPolicyGateway policyGateway,
    IClock clock) : ICommandHandler<PreviewCancelBookingCommand, BookingCancellationPreviewResponse>
{
    public async Task<Result<BookingCancellationPreviewResponse>> Handle(PreviewCancelBookingCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(CancellationInitiator), command.Initiator))
            return Result.Failure<BookingCancellationPreviewResponse>(BookingErrors.InvalidCancellationInitiator);
        if (!Enum.IsDefined(typeof(CancellationReasonCode), command.ReasonCode))
            return Result.Failure<BookingCancellationPreviewResponse>(BookingErrors.InvalidCancellationReason);

        Booking? booking = await repository.GetByIdAsync(command.BookingId, command.OrganizationId, cancellationToken);
        if (booking is null)
            return Result.Failure<BookingCancellationPreviewResponse>(BookingApplicationErrors.NotFound);
        if (booking.Status == BookingStatus.Cancelled || clock.UtcNow >= booking.StartAtUtc)
            return Result.Failure<BookingCancellationPreviewResponse>(BookingErrors.CancellationNotAllowed);

        var initiator = (CancellationInitiator)command.Initiator;
        var reasonCode = (CancellationReasonCode)command.ReasonCode;
        BookingCancellationPolicyResolution policy = await policyGateway.ResolveAsync(
            command.OrganizationId, booking, initiator, reasonCode, clock.UtcNow, cancellationToken);
        int noticeMinutes = Math.Max(0, (int)Math.Floor((booking.StartAtUtc - clock.UtcNow).TotalMinutes));

        return Result.Success(new BookingCancellationPreviewResponse(
            booking.Id.Value,
            booking.StartAtUtc,
            booking.EndAtUtc,
            command.Initiator,
            command.ReasonCode,
            noticeMinutes,
            policy.PolicyCode,
            policy.PolicyVersion,
            policy.ExplanationKey,
            (int)policy.CreditDecision,
            (int)policy.FeeDecision,
            policy.ReplacementRequired));
    }
}

public sealed class CancelBookingCommandHandler(
    IBookingRepository repository,
    IBookingCancellationPolicyGateway policyGateway,
    IBookingCapacityLock capacityLock,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CancelBookingCommand, BookingCancellationResponse>
{
    public async Task<Result<BookingCancellationResponse>> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(CancellationInitiator), command.Initiator))
            return Result.Failure<BookingCancellationResponse>(BookingErrors.InvalidCancellationInitiator);
        if (!Enum.IsDefined(typeof(CancellationReasonCode), command.ReasonCode))
            return Result.Failure<BookingCancellationResponse>(BookingErrors.InvalidCancellationReason);
        if (!Enum.IsDefined(typeof(BookingNotificationDecision), command.NotificationDecision))
            return Result.Failure<BookingCancellationResponse>(BookingErrors.InvalidCancellationDecision);

        Booking? snapshot = await repository.GetByIdAsync(command.BookingId, command.OrganizationId, cancellationToken);
        if (snapshot is null)
            return Result.Failure<BookingCancellationResponse>(BookingApplicationErrors.NotFound);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await capacityLock.AcquireAsync(command.OrganizationId, snapshot.Resources.Select(x => x.CalendarResourceId).ToArray(), cancellationToken);
            Booking? booking = await repository.GetByIdForUpdateAsync(command.BookingId, command.OrganizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingCancellationResponse>(BookingApplicationErrors.NotFound);
            }

            BookingCancellation? replay = booking.Cancellations.SingleOrDefault(x => x.OperationId == command.OperationId);
            if (replay is not null)
            {
                bool same = replay.Initiator == (CancellationInitiator)command.Initiator &&
                            replay.InitiatorId == command.InitiatorId &&
                            replay.ReasonCode == (CancellationReasonCode)command.ReasonCode &&
                            string.Equals(replay.ReasonDetails, string.IsNullOrWhiteSpace(command.ReasonDetails) ? null : command.ReasonDetails.Trim(), StringComparison.Ordinal) &&
                            replay.NotificationDecision == (BookingNotificationDecision)command.NotificationDecision &&
                            replay.OverrideApplied == command.OverrideApplied &&
                            string.Equals(replay.OverrideReason, command.OverrideReason?.Trim(), StringComparison.Ordinal);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return same
                    ? Result.Success(ToResponse(replay))
                    : Result.Failure<BookingCancellationResponse>(BookingErrors.CancellationOperationConflict);
            }

            DateTimeOffset cancelledAt = clock.UtcNow;
            var initiator = (CancellationInitiator)command.Initiator;
            var reasonCode = (CancellationReasonCode)command.ReasonCode;
            BookingCancellationPolicyResolution policy = await policyGateway.ResolveAsync(
                command.OrganizationId, booking, initiator, reasonCode, cancelledAt, cancellationToken);

            Result cancelled = booking.Cancel(
                command.OperationId,
                initiator,
                command.InitiatorId,
                reasonCode,
                command.ReasonDetails,
                cancelledAt,
                new BookingCancellationPolicyResolutionSnapshot(
                    policy.PolicyCode, policy.PolicyVersion, policy.ExplanationKey, policy.CreditDecision, policy.FeeDecision, policy.ReplacementRequired),
                (BookingNotificationDecision)command.NotificationDecision,
                command.OverrideApplied,
                command.OverrideReason);

            if (cancelled.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingCancellationResponse>(cancelled.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(ToResponse(booking.Cancellation!));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static BookingCancellationResponse ToResponse(BookingCancellation x) => new(
        x.Id.Value, x.OperationId, (int)x.Initiator, x.InitiatorId, (int)x.ReasonCode, x.ReasonDetails,
        x.CancelledAtUtc, x.NoticeDurationMinutes, x.PolicyCode, x.PolicyVersion, x.PolicyExplanationKey,
        (int)x.CreditDecision, (int)x.FeeDecision, (int)x.NotificationDecision, x.ReplacementRequired,
        x.OverrideApplied, x.OverrideReason);
}

internal static class BookingAttendancePolicy
{
    internal static (AttendanceChargeDecision Charge, AttendanceCreditDecision Credit, AttendanceFollowUpAction FollowUp) Resolve(
        AttendanceStatus status,
        AttendanceFollowUpAction requestedFollowUp)
    {
        return status switch
        {
            AttendanceStatus.Present => (AttendanceChargeDecision.None, AttendanceCreditDecision.None, requestedFollowUp),
            AttendanceStatus.LateArrival => (AttendanceChargeDecision.None, AttendanceCreditDecision.None, requestedFollowUp),
            AttendanceStatus.PartialAttendance => (AttendanceChargeDecision.PendingExternalReview, AttendanceCreditDecision.PendingExternalReview, requestedFollowUp),
            AttendanceStatus.InstructorAbsent => (AttendanceChargeDecision.NoCharge, AttendanceCreditDecision.CreditPreserved,
                requestedFollowUp == AttendanceFollowUpAction.None ? AttendanceFollowUpAction.ReplaceInstructor : requestedFollowUp),
            AttendanceStatus.UnableToDeliver => (AttendanceChargeDecision.NoCharge, AttendanceCreditDecision.CreditPreserved,
                requestedFollowUp == AttendanceFollowUpAction.None ? AttendanceFollowUpAction.Reschedule : requestedFollowUp),
            AttendanceStatus.CancelledBeforeStart => (AttendanceChargeDecision.None, AttendanceCreditDecision.None, requestedFollowUp),
            _ => (AttendanceChargeDecision.PendingExternalReview, AttendanceCreditDecision.PendingExternalReview, requestedFollowUp)
        };
    }
}

public sealed class RecordBookingAttendanceCommandHandler(
    IBookingRepository repository,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock,
    DriveOS.Application.Abstractions.Authentication.ICurrentUser currentUser) : ICommandHandler<RecordBookingAttendanceCommand, BookingAttendanceResponse>
{
    public async Task<Result<BookingAttendanceResponse>> Handle(RecordBookingAttendanceCommand command, CancellationToken cancellationToken)
    {
        return await HandleCore(command.OrganizationId, command.BookingId, command.OperationId, command.Status,
            command.ArrivalTimeUtc, command.DepartureTimeUtc, command.DelayMinutes, command.Reason, command.EvidenceDocumentId,
            command.FollowUpAction, false, null, repository, unitOfWork, clock, currentUser, cancellationToken);
    }

    internal static async Task<Result<BookingAttendanceResponse>> HandleCore(
        OrganizationId organizationId, BookingId bookingId, Guid operationId, int statusValue,
        DateTimeOffset? arrivalTimeUtc, DateTimeOffset? departureTimeUtc, int delayMinutes, string? reason,
        Guid? evidenceDocumentId, int followUpValue, bool overrideApplied, string? overrideReason,
        IBookingRepository repository, ISchedulingCapacityUnitOfWork unitOfWork, IClock clock,
        DriveOS.Application.Abstractions.Authentication.ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(AttendanceStatus), statusValue) || !Enum.IsDefined(typeof(AttendanceFollowUpAction), followUpValue))
            return Result.Failure<BookingAttendanceResponse>(BookingErrors.InvalidAttendance);
        if (currentUser.UserId is not { } userId)
            return Result.Failure<BookingAttendanceResponse>(BookingErrors.InvalidAttendanceOperation);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Booking? booking = await repository.GetByIdForUpdateAsync(bookingId, organizationId, cancellationToken);
            if (booking is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingAttendanceResponse>(BookingApplicationErrors.NotFound);
            }

            var status = (AttendanceStatus)statusValue;
            var followUp = (AttendanceFollowUpAction)followUpValue;
            (AttendanceChargeDecision charge, AttendanceCreditDecision credit, AttendanceFollowUpAction effectiveFollowUp) = BookingAttendancePolicy.Resolve(status, followUp);
            Result result = booking.RecordAttendance(operationId, status, clock.UtcNow, userId, arrivalTimeUtc, departureTimeUtc,
                delayMinutes, reason, evidenceDocumentId, charge, credit, effectiveFollowUp, overrideApplied, overrideReason);
            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<BookingAttendanceResponse>(result.Error);
            }

            BookingAttendance attendance = booking.AttendanceHistory.Single(x => x.OperationId == operationId);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(ToResponse(attendance));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    internal static BookingAttendanceResponse ToResponse(BookingAttendance x) => new(
        x.Id.Value, x.OperationId, x.SupersedesAttendanceId?.Value, (int)x.Status, x.RecordedAtUtc, x.RecordedBy.Value,
        x.ArrivalTimeUtc, x.DepartureTimeUtc, x.DelayMinutes, x.Reason, x.EvidenceDocumentId,
        (int)x.ChargeDecision, (int)x.CreditDecision, (int)x.FollowUpAction, x.OverrideApplied, x.OverrideReason);
}

public sealed class CorrectBookingAttendanceCommandHandler(
    IBookingRepository repository,
    ISchedulingCapacityUnitOfWork unitOfWork,
    IClock clock,
    DriveOS.Application.Abstractions.Authentication.ICurrentUser currentUser) : ICommandHandler<CorrectBookingAttendanceCommand, BookingAttendanceResponse>
{
    public Task<Result<BookingAttendanceResponse>> Handle(CorrectBookingAttendanceCommand command, CancellationToken cancellationToken) =>
        RecordBookingAttendanceCommandHandler.HandleCore(command.OrganizationId, command.BookingId, command.OperationId, command.Status,
            command.ArrivalTimeUtc, command.DepartureTimeUtc, command.DelayMinutes, command.Reason, command.EvidenceDocumentId,
            command.FollowUpAction, command.OverrideApplied, command.OverrideReason, repository, unitOfWork, clock, currentUser, cancellationToken);
}
