namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum BookingNotificationPolicy
{
    None = 0,
    OnConfirmation = 1,
    Standard = 2
}

public enum BookingCreditReservationStatus
{
    NotRequired = 0,
    Pending = 1,
    Reserved = 2
}

public sealed record BookingCreationDetails(
    string IdempotencyKey,
    string RequestFingerprint,
    Guid? TrainingPathId,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    string? PricingReference,
    Guid? TrainingCreditAccountId,
    decimal? CreditQuantity,
    string? Notes,
    BookingNotificationPolicy NotificationPolicy);
