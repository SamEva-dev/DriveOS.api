using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public sealed record BookingRescheduleResourceRequest(Guid CalendarResourceId, int Quantity);

public sealed record BookingRescheduleHistoryResponse(
    Guid Id,
    Guid OperationId,
    DateTimeOffset PreviousStartAtUtc,
    DateTimeOffset PreviousEndAtUtc,
    DateTimeOffset NewStartAtUtc,
    DateTimeOffset NewEndAtUtc,
    Guid? PreviousBranchId,
    Guid? NewBranchId,
    int PreviousStatus,
    string Reason,
    bool ResourcesChanged,
    string PreviousResourceFingerprint,
    string NewResourceFingerprint,
    DateTimeOffset OccurredAtUtc);

public sealed record BookingRescheduleImpactItemResponse(
    string Code,
    string State,
    string MessageKey);

public sealed record BookingRescheduleImpactResponse(
    Guid BookingId,
    Guid OperationId,
    bool AlreadyApplied,
    DateTimeOffset PreviousStartAtUtc,
    DateTimeOffset PreviousEndAtUtc,
    DateTimeOffset NewStartAtUtc,
    DateTimeOffset NewEndAtUtc,
    Guid? PreviousBranchId,
    Guid? NewBranchId,
    bool ResourcesChanged,
    bool CanConfirm,
    BookingConflictCheckResponse ConflictCheck,
    IReadOnlyCollection<BookingRescheduleImpactItemResponse> Impacts);

internal static class BookingRescheduleImpactFactory
{
    internal static BookingRescheduleImpactResponse Create(
        Booking booking,
        Guid operationId,
        DateTimeOffset previousStartAtUtc,
        DateTimeOffset previousEndAtUtc,
        BranchId? previousBranchId,
        bool resourcesChanged,
        BookingConflictAssessment assessment,
        bool alreadyApplied = false)
    {
        bool travelConflict = assessment.Conflicts.Any(x => x.Type == BookingConflictType.TravelTimeViolation);
        bool availabilityConflict = assessment.Conflicts.Any(x => x.Type is BookingConflictType.OutsideAvailability or BookingConflictType.ResourceUnavailable or BookingConflictType.ResourceRestricted);
        bool capacityConflict = assessment.Conflicts.Any(x => x.Type is BookingConflictType.OverlappingBooking or BookingConflictType.CapacityExceeded);

        BookingRescheduleImpactItemResponse[] impacts =
        [
            new("availability", availabilityConflict ? "blocked" : "compatible", availabilityConflict ? "scheduling.reschedule.impact.availability.blocked" : "scheduling.reschedule.impact.availability.compatible"),
            new("resources", resourcesChanged ? "changed" : "unchanged", resourcesChanged ? "scheduling.reschedule.impact.resources.changed" : "scheduling.reschedule.impact.resources.unchanged"),
            new("capacity", capacityConflict ? "blocked" : "compatible", capacityConflict ? "scheduling.reschedule.impact.capacity.blocked" : "scheduling.reschedule.impact.capacity.compatible"),
            new("travel", travelConflict ? "blocked" : "compatible", travelConflict ? "scheduling.reschedule.impact.travel.blocked" : "scheduling.reschedule.impact.travel.compatible"),
            new("credit",
                booking.CreditReservationStatus == BookingCreditReservationStatus.Reserved ? "preserved" : booking.CreditReservationStatus == BookingCreditReservationStatus.NotRequired ? "not-applicable" : "external-review",
                booking.CreditReservationStatus == BookingCreditReservationStatus.Reserved ? "scheduling.reschedule.impact.credit.preserved" : booking.CreditReservationStatus == BookingCreditReservationStatus.NotRequired ? "scheduling.reschedule.impact.credit.notApplicable" : "scheduling.reschedule.impact.credit.externalReview"),
            new("tariff",
                string.IsNullOrWhiteSpace(booking.PricingReference) ? "not-applicable" : "preserved",
                string.IsNullOrWhiteSpace(booking.PricingReference) ? "scheduling.reschedule.impact.tariff.notApplicable" : "scheduling.reschedule.impact.tariff.preserved"),
            new("cancellation-policy", "external-review", "scheduling.reschedule.impact.cancellationPolicy.externalReview"),
            new("objectives", string.IsNullOrWhiteSpace(booking.Objectives) ? "not-applicable" : "preserved", string.IsNullOrWhiteSpace(booking.Objectives) ? "scheduling.reschedule.impact.objectives.notApplicable" : "scheduling.reschedule.impact.objectives.preserved"),
            new("partner-mission", "external-review", "scheduling.reschedule.impact.partnerMission.externalReview"),
            new("exam", booking.BookingType == BookingType.Exam ? "external-review" : "not-applicable", booking.BookingType == BookingType.Exam ? "scheduling.reschedule.impact.exam.externalReview" : "scheduling.reschedule.impact.exam.notApplicable"),
            new("remuneration", "external-review", "scheduling.reschedule.impact.remuneration.externalReview"),
            new("notifications", booking.NotificationPolicy == BookingNotificationPolicy.None ? "disabled" : "required", booking.NotificationPolicy == BookingNotificationPolicy.None ? "scheduling.reschedule.impact.notifications.disabled" : "scheduling.reschedule.impact.notifications.required")
        ];

        return new BookingRescheduleImpactResponse(
            booking.Id.Value,
            operationId,
            alreadyApplied,
            previousStartAtUtc,
            previousEndAtUtc,
            booking.StartAtUtc,
            booking.EndAtUtc,
            previousBranchId?.Value,
            booking.BranchId?.Value,
            resourcesChanged,
            assessment.IsConflictFree,
            assessment.ToResponse(),
            impacts);
    }
    internal static BookingRescheduleImpactResponse CreateReplay(
        Booking booking,
        BookingRescheduleHistory history)
    {
        BookingConflictCheckResponse conflictCheck = new(
            booking.Id.Value,
            history.NewStartAtUtc,
            history.NewEndAtUtc,
            true,
            []);

        BookingRescheduleImpactItemResponse[] impacts =
        [
            new("idempotency", "already-applied", "scheduling.reschedule.impact.idempotency.alreadyApplied"),
            new("resources", history.ResourcesChanged ? "changed" : "unchanged", history.ResourcesChanged ? "scheduling.reschedule.impact.resources.changed" : "scheduling.reschedule.impact.resources.unchanged"),
            new("notifications", "previously-requested", "scheduling.reschedule.impact.notifications.previouslyRequested")
        ];

        return new BookingRescheduleImpactResponse(
            booking.Id.Value,
            history.OperationId,
            true,
            history.PreviousStartAtUtc,
            history.PreviousEndAtUtc,
            history.NewStartAtUtc,
            history.NewEndAtUtc,
            history.PreviousBranchId?.Value,
            history.NewBranchId?.Value,
            history.ResourcesChanged,
            true,
            conflictCheck,
            impacts);
    }

}
