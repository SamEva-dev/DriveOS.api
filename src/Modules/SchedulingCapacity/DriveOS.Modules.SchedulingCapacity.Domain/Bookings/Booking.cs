using DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class Booking : AggregateRoot<BookingId>, IAuditableEntity
{
    private readonly List<BookingResource> resources = [];
    private readonly List<BookingParticipant> participants = [];
    private readonly List<BookingRescheduleHistory> rescheduleHistory = [];
    private readonly List<BookingCancellation> cancellations = [];
    private readonly List<BookingAttendance> attendanceHistory = [];
    private readonly List<BookingInstructorReplacement> instructorReplacementHistory = [];
    private readonly List<BookingVehicleReplacement> vehicleReplacementHistory = [];

    private Booking() { }

    private Booking(
        BookingId id,
        OrganizationId organizationId,
        BranchId? branchId,
        BookingType bookingType,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        string title,
        BookingCreationDetails details)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        BookingType = bookingType;
        StartAtUtc = startAtUtc.ToUniversalTime();
        EndAtUtc = endAtUtc.ToUniversalTime();
        Title = title;
        CreationIdempotencyKey = details.IdempotencyKey;
        CreationRequestFingerprint = details.RequestFingerprint;
        TrainingPathId = details.TrainingPathId;
        TrainingCategory = NormalizeOptional(details.TrainingCategory);
        Objectives = NormalizeOptional(details.Objectives);
        MeetingPoint = NormalizeOptional(details.MeetingPoint);
        PricingReference = NormalizeOptional(details.PricingReference);
        TrainingCreditAccountId = details.TrainingCreditAccountId;
        CreditQuantity = details.CreditQuantity;
        CreditReservationStatus = details.TrainingCreditAccountId.HasValue
            ? BookingCreditReservationStatus.Pending
            : BookingCreditReservationStatus.NotRequired;
        Notes = NormalizeOptional(details.Notes);
        NotificationPolicy = details.NotificationPolicy;
        Status = BookingStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public BookingType BookingType { get; private set; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string CreationIdempotencyKey { get; private set; } = string.Empty;
    public string CreationRequestFingerprint { get; private set; } = string.Empty;
    public Guid? TrainingPathId { get; private set; }
    public string? TrainingCategory { get; private set; }
    public string? Objectives { get; private set; }
    public string? MeetingPoint { get; private set; }
    public string? PricingReference { get; private set; }
    public Guid? TrainingCreditAccountId { get; private set; }
    public decimal? CreditQuantity { get; private set; }
    public BookingCreditReservationStatus CreditReservationStatus { get; private set; }
    public string? CreditReservationReference { get; private set; }
    public string? Notes { get; private set; }
    public BookingNotificationPolicy NotificationPolicy { get; private set; }
    public DateTimeOffset? HoldExpiresAtUtc { get; private set; }
    public BookingStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public IReadOnlyCollection<BookingCancellation> Cancellations => cancellations;
    public IReadOnlyCollection<BookingAttendance> AttendanceHistory => attendanceHistory;
    public BookingAttendance? CurrentAttendance => attendanceHistory.OrderByDescending(x => x.RecordedAtUtc).ThenByDescending(x => x.Id.Value).FirstOrDefault();
    public BookingCancellation? Cancellation => cancellations.OrderByDescending(x => x.CancelledAtUtc).FirstOrDefault();
    public IReadOnlyCollection<BookingResource> Resources => resources;
    public IReadOnlyCollection<BookingParticipant> Participants => participants;
    public IReadOnlyCollection<BookingRescheduleHistory> RescheduleHistory => rescheduleHistory;
    public IReadOnlyCollection<BookingInstructorReplacement> InstructorReplacementHistory => instructorReplacementHistory;
    public IReadOnlyCollection<BookingVehicleReplacement> VehicleReplacementHistory => vehicleReplacementHistory;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Booking> Create(
        BookingId id,
        OrganizationId organizationId,
        BranchId? branchId,
        BookingType bookingType,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        string title) =>
        Create(
            id,
            organizationId,
            branchId,
            bookingType,
            startAtUtc,
            endAtUtc,
            title,
            new BookingCreationDetails(
                $"internal:{id.Value:N}",
                new string('0', 64),
                null, null, null, null, null, null, null, null, BookingNotificationPolicy.None));

    public static Result<Booking> Create(
        BookingId id,
        OrganizationId organizationId,
        BranchId? branchId,
        BookingType bookingType,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        string title,
        BookingCreationDetails details)
    {
        if (id.IsEmpty)
            return Result.Failure<Booking>(BookingErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<Booking>(BookingErrors.InvalidOrganization);
        if (branchId.HasValue && branchId.Value.IsEmpty)
            return Result.Failure<Booking>(BookingErrors.InvalidBranch);
        if (!Enum.IsDefined(bookingType))
            return Result.Failure<Booking>(BookingErrors.InvalidType);
        if (endAtUtc <= startAtUtc)
            return Result.Failure<Booking>(BookingErrors.InvalidPeriod);

        string normalizedTitle = title?.Trim() ?? string.Empty;
        if (normalizedTitle.Length is < 1 or > 200)
            return Result.Failure<Booking>(BookingErrors.InvalidTitle);

        if (details is null ||
            string.IsNullOrWhiteSpace(details.IdempotencyKey) || details.IdempotencyKey.Trim().Length > 120 ||
            string.IsNullOrWhiteSpace(details.RequestFingerprint) || details.RequestFingerprint.Trim().Length != 64 ||
            details.TrainingPathId == Guid.Empty ||
            details.TrainingCreditAccountId == Guid.Empty ||
            details.CreditQuantity is <= 0m ||
            details.TrainingCategory?.Trim().Length > 80 ||
            details.Objectives?.Trim().Length > 2000 ||
            details.MeetingPoint?.Trim().Length > 500 ||
            details.PricingReference?.Trim().Length > 200 ||
            details.Notes?.Trim().Length > 2000 ||
            !Enum.IsDefined(details.NotificationPolicy) ||
            details.TrainingCreditAccountId.HasValue != details.CreditQuantity.HasValue)
            return Result.Failure<Booking>(BookingErrors.InvalidCreationDetails);

        var normalizedDetails = details with
        {
            IdempotencyKey = details.IdempotencyKey.Trim(),
            RequestFingerprint = details.RequestFingerprint.Trim().ToLowerInvariant()
        };

        var booking = new Booking(
            id,
            organizationId,
            branchId,
            bookingType,
            startAtUtc,
            endAtUtc,
            normalizedTitle,
            normalizedDetails);

        booking.RaiseDomainEvent(new BookingCreatedDomainEvent(
            booking.Id,
            booking.OrganizationId,
            booking.BookingType,
            booking.StartAtUtc,
            booking.EndAtUtc));

        return Result.Success(booking);
    }

    public Result<BookingResourceId> AddResource(
        BookingResourceId bookingResourceId,
        CalendarResourceId calendarResourceId,
        int quantity = 1)
    {
        if (Status != BookingStatus.Draft)
            return Result.Failure<BookingResourceId>(BookingErrors.ModificationNotAllowed);
        if (bookingResourceId.IsEmpty || calendarResourceId.IsEmpty || quantity is < 1 or > 10000)
            return Result.Failure<BookingResourceId>(BookingErrors.InvalidResource);
        if (resources.Any(x => x.CalendarResourceId == calendarResourceId))
            return Result.Failure<BookingResourceId>(BookingErrors.DuplicateResource);

        resources.Add(new BookingResource(bookingResourceId, Id, calendarResourceId, quantity));
        return Result.Success(bookingResourceId);
    }

    public Result<BookingParticipantId> AddParticipant(
        BookingParticipantId bookingParticipantId,
        BookingParticipantType participantType,
        Guid externalParticipantId)
    {
        if (Status != BookingStatus.Draft)
            return Result.Failure<BookingParticipantId>(BookingErrors.ModificationNotAllowed);
        if (bookingParticipantId.IsEmpty || externalParticipantId == Guid.Empty || !Enum.IsDefined(participantType))
            return Result.Failure<BookingParticipantId>(BookingErrors.InvalidParticipant);
        if (participants.Any(x => x.ParticipantType == participantType && x.ExternalParticipantId == externalParticipantId))
            return Result.Failure<BookingParticipantId>(BookingErrors.DuplicateParticipant);

        participants.Add(new BookingParticipant(bookingParticipantId, Id, participantType, externalParticipantId));
        return Result.Success(bookingParticipantId);
    }

    public Result Reschedule(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) =>
        Reschedule(Guid.NewGuid(), startAtUtc, endAtUtc, BranchId, "rescheduled", false, ResourceFingerprint(Resources), DateTimeOffset.UtcNow);

    public Result Reschedule(
        Guid operationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        BranchId? newBranchId,
        string reason,
        bool resourcesChanged,
        string newResourceFingerprint,
        DateTimeOffset occurredAtUtc)
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(BookingErrors.ModificationNotAllowed);
        if (operationId == Guid.Empty)
            return Result.Failure(BookingErrors.InvalidRescheduleOperation);
        if (endAtUtc <= startAtUtc)
            return Result.Failure(BookingErrors.InvalidPeriod);
        if (newBranchId.HasValue && newBranchId.Value.IsEmpty)
            return Result.Failure(BookingErrors.InvalidBranch);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 3 or > 500)
            return Result.Failure(BookingErrors.InvalidRescheduleReason);

        DateTimeOffset normalizedStart = startAtUtc.ToUniversalTime();
        DateTimeOffset normalizedEnd = endAtUtc.ToUniversalTime();
        BookingRescheduleHistory? existing = rescheduleHistory.SingleOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
        {
            bool sameRequest = existing.NewStartAtUtc == normalizedStart &&
                               existing.NewEndAtUtc == normalizedEnd &&
                               existing.NewBranchId == newBranchId &&
                               string.Equals(existing.Reason, normalizedReason, StringComparison.Ordinal) &&
                               string.Equals(existing.NewResourceFingerprint, newResourceFingerprint, StringComparison.Ordinal);
            return sameRequest
                ? Result.Success()
                : Result.Failure(BookingErrors.RescheduleOperationConflict);
        }

        string previousResourceFingerprint = ResourceFingerprint(Resources);
        DateTimeOffset previousStartAtUtc = StartAtUtc;
        DateTimeOffset previousEndAtUtc = EndAtUtc;
        BranchId? previousBranchId = BranchId;
        BookingStatus previousStatus = Status;

        StartAtUtc = normalizedStart;
        EndAtUtc = normalizedEnd;
        BranchId = newBranchId;
        Status = BookingStatus.Draft;

        rescheduleHistory.Add(new BookingRescheduleHistory(
            BookingRescheduleId.New(),
            Id,
            operationId,
            previousStartAtUtc,
            previousEndAtUtc,
            StartAtUtc,
            EndAtUtc,
            previousBranchId,
            BranchId,
            previousStatus,
            normalizedReason,
            resourcesChanged,
            previousResourceFingerprint,
            newResourceFingerprint,
            occurredAtUtc));

        RaiseDomainEvent(new BookingRescheduledDomainEvent(
            Id,
            OrganizationId,
            operationId,
            previousStartAtUtc,
            previousEndAtUtc,
            StartAtUtc,
            EndAtUtc,
            previousBranchId,
            BranchId,
            previousStatus,
            normalizedReason,
            resourcesChanged));

        RaiseDomainEvent(new BookingRescheduleNotificationRequestedDomainEvent(
            Id,
            OrganizationId,
            operationId,
            previousStartAtUtc,
            previousEndAtUtc,
            StartAtUtc,
            EndAtUtc,
            participants.Select(x => x.ExternalParticipantId).Distinct().ToArray()));

        return Result.Success();
    }


    public Result ReplaceInstructor(
        Guid operationId,
        CalendarResourceId previousResourceId,
        CalendarResourceId replacementResourceId,
        UserId previousInstructorId,
        UserId replacementInstructorId,
        InstructorReplacementMode mode,
        string reason,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset? accessExpiresAtUtc)
    {
        if (operationId == Guid.Empty || previousResourceId.IsEmpty || replacementResourceId.IsEmpty ||
            previousInstructorId.IsEmpty || replacementInstructorId.IsEmpty)
            return Result.Failure(BookingErrors.InvalidInstructorReplacement);
        if (!Enum.IsDefined(mode) || previousInstructorId == replacementInstructorId || previousResourceId == replacementResourceId)
            return Result.Failure(BookingErrors.InvalidInstructorReplacement);
        if (Status is not (BookingStatus.Draft or BookingStatus.Reserved or BookingStatus.Confirmed))
            return Result.Failure(BookingErrors.InstructorReplacementNotAllowed);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 1 or > 500)
            return Result.Failure(BookingErrors.InvalidInstructorReplacementReason);
        if (accessExpiresAtUtc.HasValue && accessExpiresAtUtc.Value.ToUniversalTime() <= occurredAtUtc.ToUniversalTime())
            return Result.Failure(BookingErrors.InvalidInstructorReplacementAccessExpiry);

        BookingInstructorReplacement? existing = instructorReplacementHistory.SingleOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
        {
            bool same = existing.PreviousInstructorId == previousInstructorId &&
                        existing.ReplacementInstructorId == replacementInstructorId &&
                        existing.PreviousResourceId == previousResourceId &&
                        existing.ReplacementResourceId == replacementResourceId &&
                        existing.Mode == mode &&
                        string.Equals(existing.Reason, normalizedReason, StringComparison.Ordinal) &&
                        existing.AccessExpiresAtUtc == accessExpiresAtUtc?.ToUniversalTime();
            return same ? Result.Success() : Result.Failure(BookingErrors.InstructorReplacementIdempotencyConflict);
        }

        BookingResource? instructorResource = resources.SingleOrDefault(x => x.CalendarResourceId == previousResourceId);
        if (instructorResource is null)
            return Result.Failure(BookingErrors.PreviousInstructorResourceNotFound);
        if (resources.Any(x => x.CalendarResourceId == replacementResourceId))
            return Result.Failure(BookingErrors.DuplicateResource);

        BookingParticipant? instructorParticipant = participants.SingleOrDefault(x =>
            x.ParticipantType == BookingParticipantType.Instructor && x.ExternalParticipantId == previousInstructorId.Value);

        instructorResource.ReplaceCalendarResource(replacementResourceId);
        instructorParticipant?.ReplaceExternalParticipant(replacementInstructorId.Value);

        instructorReplacementHistory.Add(new BookingInstructorReplacement(
            BookingInstructorReplacementId.New(), Id, operationId, previousInstructorId, replacementInstructorId,
            previousResourceId, replacementResourceId, mode, normalizedReason, occurredAtUtc, accessExpiresAtUtc));

        RaiseDomainEvent(new BookingInstructorReplacedDomainEvent(
            Id, OrganizationId, operationId, previousInstructorId, replacementInstructorId, accessExpiresAtUtc));
        RaiseDomainEvent(new InstructorReplacementAccessChangeRequestedDomainEvent(
            Id, OrganizationId, previousInstructorId, replacementInstructorId, occurredAtUtc, accessExpiresAtUtc));
        RaiseDomainEvent(new InstructorReplacementNotificationRequestedDomainEvent(
            Id, OrganizationId, previousInstructorId, replacementInstructorId,
            participants.Select(x => x.ExternalParticipantId).Distinct().ToArray()));

        return Result.Success();
    }

    public Result ReplaceVehicle(
        Guid operationId,
        CalendarResourceId previousResourceId,
        CalendarResourceId replacementResourceId,
        Guid previousVehicleId,
        Guid replacementVehicleId,
        VehicleReplacementMode mode,
        string reason,
        DateTimeOffset occurredAtUtc)
    {
        if (operationId == Guid.Empty || previousResourceId.IsEmpty || replacementResourceId.IsEmpty ||
            previousVehicleId == Guid.Empty || replacementVehicleId == Guid.Empty || previousVehicleId == replacementVehicleId ||
            previousResourceId == replacementResourceId || !Enum.IsDefined(mode))
            return Result.Failure(BookingErrors.InvalidVehicleReplacement);
        if (Status is not (BookingStatus.Draft or BookingStatus.Reserved or BookingStatus.Confirmed))
            return Result.Failure(BookingErrors.VehicleReplacementNotAllowed);

        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 1 or > 500)
            return Result.Failure(BookingErrors.InvalidVehicleReplacementReason);

        BookingVehicleReplacement? existing = vehicleReplacementHistory.SingleOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
        {
            bool same = existing.PreviousVehicleId == previousVehicleId && existing.ReplacementVehicleId == replacementVehicleId &&
                        existing.PreviousResourceId == previousResourceId && existing.ReplacementResourceId == replacementResourceId &&
                        existing.Mode == mode && string.Equals(existing.Reason, normalizedReason, StringComparison.Ordinal);
            return same ? Result.Success() : Result.Failure(BookingErrors.VehicleReplacementIdempotencyConflict);
        }

        BookingResource? vehicleResource = resources.SingleOrDefault(x => x.CalendarResourceId == previousResourceId);
        if (vehicleResource is null)
            return Result.Failure(BookingErrors.PreviousVehicleResourceNotFound);
        if (resources.Any(x => x.CalendarResourceId == replacementResourceId))
            return Result.Failure(BookingErrors.DuplicateResource);

        vehicleResource.ReplaceCalendarResource(replacementResourceId);
        vehicleReplacementHistory.Add(new BookingVehicleReplacement(
            BookingVehicleReplacementId.New(), Id, operationId, previousVehicleId, replacementVehicleId, previousResourceId, replacementResourceId, mode, normalizedReason, occurredAtUtc));

        RaiseDomainEvent(new BookingVehicleReplacedDomainEvent(Id, OrganizationId, operationId, previousVehicleId, replacementVehicleId, previousResourceId, replacementResourceId));
        RaiseDomainEvent(new VehicleReplacementNotificationRequestedDomainEvent(
            Id, OrganizationId, operationId, previousVehicleId, replacementVehicleId, participants.Select(x => x.ExternalParticipantId).Distinct().ToArray()));

        return Result.Success();
    }

    public Result ReplaceResources(IReadOnlyCollection<(BookingResourceId Id, CalendarResourceId ResourceId, int Quantity)> replacements)
    {
        if (Status != BookingStatus.Draft)
            return Result.Failure(BookingErrors.ModificationNotAllowed);
        if (replacements.Count == 0 || replacements.Any(x => x.Id.IsEmpty || x.ResourceId.IsEmpty || x.Quantity is < 1 or > 10000))
            return Result.Failure(BookingErrors.InvalidResource);
        if (replacements.Select(x => x.ResourceId).Distinct().Count() != replacements.Count)
            return Result.Failure(BookingErrors.DuplicateResource);

        resources.Clear();
        foreach ((BookingResourceId id, CalendarResourceId resourceId, int quantity) in replacements)
            resources.Add(new BookingResource(id, Id, resourceId, quantity));

        return Result.Success();
    }

    public Result Hold(BookingConflictAssessment assessment, DateTimeOffset expiresAtUtc, DateTimeOffset nowUtc)
    {
        if (Status is not (BookingStatus.Draft or BookingStatus.Tentative))
            return Result.Failure(BookingErrors.ReservationNotAllowed);
        if (resources.Count == 0)
            return Result.Failure(BookingErrors.ResourcesRequired);
        if (assessment.BookingId != Id ||
            assessment.StartAtUtc.ToUniversalTime() != StartAtUtc ||
            assessment.EndAtUtc.ToUniversalTime() != EndAtUtc)
            return Result.Failure(BookingErrors.ConflictCheckRequired);
        if (!assessment.IsConflictFree)
            return Result.Failure(BookingErrors.ResourceConflict);

        DateTimeOffset normalizedNow = nowUtc.ToUniversalTime();
        DateTimeOffset normalizedExpiry = expiresAtUtc.ToUniversalTime();
        if (normalizedExpiry <= normalizedNow || normalizedExpiry > normalizedNow.AddMinutes(15))
            return Result.Failure(BookingErrors.InvalidSlotHold);

        Status = BookingStatus.Tentative;
        HoldExpiresAtUtc = normalizedExpiry;
        RaiseDomainEvent(new BookingSlotHeldDomainEvent(Id, OrganizationId, normalizedExpiry));
        return Result.Success();
    }

    public Result Reserve(BookingConflictAssessment assessment)
    {
        if (Status is not (BookingStatus.Draft or BookingStatus.Tentative))
            return Result.Failure(BookingErrors.ReservationNotAllowed);
        if (resources.Count == 0)
            return Result.Failure(BookingErrors.ResourcesRequired);
        if (assessment.BookingId != Id ||
            assessment.StartAtUtc.ToUniversalTime() != StartAtUtc ||
            assessment.EndAtUtc.ToUniversalTime() != EndAtUtc)
            return Result.Failure(BookingErrors.ConflictCheckRequired);
        if (!assessment.IsConflictFree)
            return Result.Failure(BookingErrors.ResourceConflict);

        Status = BookingStatus.Reserved;
        HoldExpiresAtUtc = null;
        RaiseDomainEvent(new BookingReservedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result MarkCreditReserved(string reservationReference)
    {
        if (CreditReservationStatus == BookingCreditReservationStatus.NotRequired)
            return Result.Success();

        string normalizedReference = reservationReference?.Trim() ?? string.Empty;
        if (normalizedReference.Length is < 3 or > 200)
            return Result.Failure(BookingErrors.CreditReservationFailed);

        if (CreditReservationStatus == BookingCreditReservationStatus.Reserved)
            return string.Equals(CreditReservationReference, normalizedReference, StringComparison.Ordinal)
                ? Result.Success()
                : Result.Failure(BookingErrors.CreditReservationFailed);

        CreditReservationReference = normalizedReference;
        CreditReservationStatus = BookingCreditReservationStatus.Reserved;
        return Result.Success();
    }

    public Result Confirm()
    {
        if (Status != BookingStatus.Reserved)
            return Result.Failure(BookingErrors.ConfirmationNotAllowed);
        if (CreditReservationStatus == BookingCreditReservationStatus.Pending)
            return Result.Failure(BookingErrors.CreditReservationRequired);

        Status = BookingStatus.Confirmed;
        RaiseDomainEvent(new BookingConfirmedDomainEvent(Id, OrganizationId));
        if (NotificationPolicy != BookingNotificationPolicy.None)
        {
            RaiseDomainEvent(new BookingNotificationRequestedDomainEvent(
                Id,
                OrganizationId,
                NotificationPolicy,
                participants.Select(x => x.ExternalParticipantId).Distinct().ToArray()));
        }
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        return Cancel(
            Guid.NewGuid(),
            CancellationInitiator.Organization,
            null,
            CancellationReasonCode.Other,
            reason,
            DateTimeOffset.UtcNow,
            new BookingCancellationPolicyResolutionSnapshot(
                "legacy",
                1,
                "scheduling.cancellation.policy.legacy",
                BookingCreditDecision.PendingExternalReview,
                BookingFeeDecision.PendingExternalReview,
                false),
            BookingNotificationDecision.NotifyAffectedParticipants,
            false,
            null);
    }

    public Result Cancel(
        Guid operationId,
        CancellationInitiator initiator,
        Guid? initiatorId,
        CancellationReasonCode reasonCode,
        string? reasonDetails,
        DateTimeOffset cancelledAtUtc,
        BookingCancellationPolicyResolutionSnapshot policy,
        BookingNotificationDecision notificationDecision,
        bool overrideApplied,
        string? overrideReason)
    {
        if (operationId == Guid.Empty)
            return Result.Failure(BookingErrors.InvalidCancellationOperation);
        if (!Enum.IsDefined(initiator))
            return Result.Failure(BookingErrors.InvalidCancellationInitiator);
        if (!Enum.IsDefined(reasonCode))
            return Result.Failure(BookingErrors.InvalidCancellationReason);
        if (!Enum.IsDefined(notificationDecision))
            return Result.Failure(BookingErrors.InvalidCancellationDecision);
        if (cancelledAtUtc.ToUniversalTime() >= StartAtUtc)
            return Result.Failure(BookingErrors.CancellationAfterStartNotAllowed);

        string? normalizedDetails = string.IsNullOrWhiteSpace(reasonDetails) ? null : reasonDetails.Trim();
        if (normalizedDetails?.Length > 500)
            return Result.Failure(BookingErrors.InvalidCancellationReason);
        if (reasonCode == CancellationReasonCode.Other && normalizedDetails is null)
            return Result.Failure(BookingErrors.CancellationReasonDetailsRequired);
        if (string.IsNullOrWhiteSpace(policy.PolicyCode) || string.IsNullOrWhiteSpace(policy.ExplanationKey) || policy.PolicyVersion < 1)
            return Result.Failure(BookingErrors.InvalidCancellationPolicy);
        if (overrideApplied && string.IsNullOrWhiteSpace(overrideReason))
            return Result.Failure(BookingErrors.CancellationOverrideReasonRequired);
        if (!overrideApplied && !string.IsNullOrWhiteSpace(overrideReason))
            return Result.Failure(BookingErrors.InvalidCancellationOverride);

        BookingCancellation? existingOperation = cancellations.SingleOrDefault(x => x.OperationId == operationId);
        if (existingOperation is not null)
        {
            bool same = existingOperation.Initiator == initiator &&
                        existingOperation.InitiatorId == initiatorId &&
                        existingOperation.ReasonCode == reasonCode &&
                        string.Equals(existingOperation.ReasonDetails, normalizedDetails, StringComparison.Ordinal) &&
                        existingOperation.PolicyCode == policy.PolicyCode &&
                        existingOperation.PolicyVersion == policy.PolicyVersion &&
                        existingOperation.CreditDecision == policy.CreditDecision &&
                        existingOperation.FeeDecision == policy.FeeDecision &&
                        existingOperation.NotificationDecision == notificationDecision &&
                        existingOperation.OverrideApplied == overrideApplied &&
                        string.Equals(existingOperation.OverrideReason, overrideReason?.Trim(), StringComparison.Ordinal);
            return same ? Result.Success() : Result.Failure(BookingErrors.CancellationOperationConflict);
        }

        if (Status == BookingStatus.Cancelled || cancellations.Count > 0)
            return Result.Failure(BookingErrors.CancellationNotAllowed);
        if (Status is not BookingStatus.Draft and not BookingStatus.Reserved and not BookingStatus.Confirmed)
            return Result.Failure(BookingErrors.CancellationNotAllowed);

        DateTimeOffset normalizedCancelledAt = cancelledAtUtc.ToUniversalTime();
        int noticeMinutes = Math.Max(0, (int)Math.Floor((StartAtUtc - normalizedCancelledAt).TotalMinutes));
        BookingCancellationId cancellationId = BookingCancellationId.New();
        var cancellation = new BookingCancellation(
            cancellationId,
            Id,
            operationId,
            initiator,
            initiatorId,
            reasonCode,
            normalizedDetails,
            normalizedCancelledAt,
            noticeMinutes,
            policy.PolicyCode.Trim(),
            policy.PolicyVersion,
            policy.ExplanationKey.Trim(),
            policy.CreditDecision,
            policy.FeeDecision,
            notificationDecision,
            policy.ReplacementRequired,
            overrideApplied,
            overrideReason?.Trim());

        BookingStatus previousStatus = Status;
        cancellations.Add(cancellation);
        Status = BookingStatus.Cancelled;
        CancellationReason = reasonCode.ToString();

        RaiseDomainEvent(new BookingCancelledDomainEvent(Id, OrganizationId, cancellationId, operationId, initiator, reasonCode, normalizedDetails, normalizedCancelledAt));
        RaiseDomainEvent(new BookingCancellationConsequencesRequestedDomainEvent(Id, OrganizationId, cancellationId, policy.CreditDecision, policy.FeeDecision));
        if (previousStatus is BookingStatus.Reserved or BookingStatus.Confirmed)
            RaiseDomainEvent(new BookingSlotReleasedDomainEvent(Id, OrganizationId, BranchId, StartAtUtc, EndAtUtc, resources.Select(x => x.CalendarResourceId).Distinct().ToArray()));
        RaiseDomainEvent(new BookingCancellationNotificationRequestedDomainEvent(Id, OrganizationId, cancellationId, notificationDecision, participants.Select(x => x.ExternalParticipantId).Distinct().ToArray()));
        return Result.Success();
    }

    public Result RecordAttendance(
        Guid operationId,
        AttendanceStatus status,
        DateTimeOffset recordedAtUtc,
        UserId recordedBy,
        DateTimeOffset? arrivalTimeUtc,
        DateTimeOffset? departureTimeUtc,
        int delayMinutes,
        string? reason,
        Guid? evidenceDocumentId,
        AttendanceChargeDecision chargeDecision,
        AttendanceCreditDecision creditDecision,
        AttendanceFollowUpAction followUpAction,
        bool overrideApplied,
        string? overrideReason)
    {
        if (operationId == Guid.Empty || recordedBy.IsEmpty)
            return Result.Failure(BookingErrors.InvalidAttendanceOperation);
        if (!Enum.IsDefined(status) || !Enum.IsDefined(chargeDecision) || !Enum.IsDefined(creditDecision) || !Enum.IsDefined(followUpAction))
            return Result.Failure(BookingErrors.InvalidAttendance);
        if (Status == BookingStatus.Cancelled && status != AttendanceStatus.CancelledBeforeStart)
            return Result.Failure(BookingErrors.AttendanceNotAllowed);
        if (Status is not BookingStatus.Reserved and not BookingStatus.Confirmed and not BookingStatus.Cancelled)
            return Result.Failure(BookingErrors.AttendanceNotAllowed);
        if (recordedAtUtc.ToUniversalTime() < StartAtUtc.AddMinutes(-30))
            return Result.Failure(BookingErrors.AttendanceTooEarly);
        if (delayMinutes is < 0 or > 1440)
            return Result.Failure(BookingErrors.InvalidAttendance);
        if (status == AttendanceStatus.LateArrival && delayMinutes <= 0)
            return Result.Failure(BookingErrors.AttendanceDelayRequired);
        if (status != AttendanceStatus.LateArrival && delayMinutes > 0 && arrivalTimeUtc is null)
            return Result.Failure(BookingErrors.InvalidAttendance);
        if (arrivalTimeUtc.HasValue && departureTimeUtc.HasValue && departureTimeUtc.Value <= arrivalTimeUtc.Value)
            return Result.Failure(BookingErrors.InvalidActualPeriod);
        if (status is AttendanceStatus.Present or AttendanceStatus.LateArrival or AttendanceStatus.PartialAttendance)
        {
            if (arrivalTimeUtc is null)
                return Result.Failure(BookingErrors.ArrivalTimeRequired);
        }
        string? normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (normalizedReason?.Length > 500)
            return Result.Failure(BookingErrors.InvalidAttendanceReason);
        if (overrideApplied && string.IsNullOrWhiteSpace(overrideReason))
            return Result.Failure(BookingErrors.AttendanceOverrideReasonRequired);
        if (!overrideApplied && !string.IsNullOrWhiteSpace(overrideReason))
            return Result.Failure(BookingErrors.InvalidAttendanceOverride);

        BookingAttendance? existingOperation = attendanceHistory.SingleOrDefault(x => x.OperationId == operationId);
        if (existingOperation is not null)
        {
            bool same = existingOperation.Status == status &&
                        existingOperation.RecordedBy == recordedBy &&
                        existingOperation.ArrivalTimeUtc == arrivalTimeUtc?.ToUniversalTime() &&
                        existingOperation.DepartureTimeUtc == departureTimeUtc?.ToUniversalTime() &&
                        existingOperation.DelayMinutes == delayMinutes &&
                        string.Equals(existingOperation.Reason, normalizedReason, StringComparison.Ordinal) &&
                        existingOperation.EvidenceDocumentId == evidenceDocumentId &&
                        existingOperation.ChargeDecision == chargeDecision &&
                        existingOperation.CreditDecision == creditDecision &&
                        existingOperation.FollowUpAction == followUpAction &&
                        existingOperation.OverrideApplied == overrideApplied &&
                        string.Equals(existingOperation.OverrideReason, overrideReason?.Trim(), StringComparison.Ordinal);
            return same ? Result.Success() : Result.Failure(BookingErrors.AttendanceOperationConflict);
        }

        BookingAttendance? current = CurrentAttendance;
        DateTimeOffset now = recordedAtUtc.ToUniversalTime();
        if (current is not null && !overrideApplied && now > current.RecordedAtUtc.AddHours(24))
            return Result.Failure(BookingErrors.AttendanceCorrectionWindowExpired);

        var attendance = new BookingAttendance(
            BookingAttendanceId.New(),
            Id,
            operationId,
            current?.Id,
            status,
            now,
            recordedBy,
            arrivalTimeUtc,
            departureTimeUtc,
            delayMinutes,
            normalizedReason,
            evidenceDocumentId,
            chargeDecision,
            creditDecision,
            followUpAction,
            overrideApplied,
            overrideReason?.Trim());

        attendanceHistory.Add(attendance);

        if (current is null)
            RaiseDomainEvent(new BookingAttendanceRecordedDomainEvent(Id, OrganizationId, attendance.Id, attendance.Status, attendance.RecordedAtUtc));
        else
            RaiseDomainEvent(new BookingAttendanceCorrectedDomainEvent(Id, OrganizationId, current.Id, attendance.Id, attendance.Status, overrideApplied));

        if (chargeDecision != AttendanceChargeDecision.None || creditDecision != AttendanceCreditDecision.None)
            RaiseDomainEvent(new BookingAttendanceConsequencesRequestedDomainEvent(Id, OrganizationId, attendance.Id, chargeDecision, creditDecision));

        return Result.Success();
    }

    public static string ResourceFingerprint(IEnumerable<BookingResource> bookingResources) =>
        string.Join("|", bookingResources
            .OrderBy(x => x.CalendarResourceId.Value)
            .Select(x => $"{x.CalendarResourceId.Value:N}:{x.Quantity}"));

    public bool Overlaps(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) =>
        BookingConflictDetector.Overlaps(StartAtUtc, EndAtUtc, startAtUtc, endAtUtc);

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

}
