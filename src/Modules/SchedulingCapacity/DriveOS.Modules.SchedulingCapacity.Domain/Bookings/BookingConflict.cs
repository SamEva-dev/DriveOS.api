using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public enum BookingConflictType
{
    ResourceUnavailable = 1,
    ResourceRestricted = 2,
    OutsideAvailability = 3,
    CapacityExceeded = 4,
    OverlappingBooking = 5,
    TransitionBufferViolation = 6,
    TravelTimeViolation = 7
}

public sealed record BookingConflict(
    BookingConflictType Type,
    CalendarResourceId CalendarResourceId,
    BookingId? ConflictingBookingId,
    int RequestedQuantity,
    int AvailableCapacity,
    string? Reason = null);

public sealed record BookingConflictAssessment(
    BookingId BookingId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    IReadOnlyCollection<BookingConflict> Conflicts)
{
    public bool IsConflictFree => Conflicts.Count == 0;
}

public sealed record CalendarResourceSchedulingSnapshot(
    CalendarResourceId ResourceId,
    int ResourceCapacity,
    int EffectiveCapacity,
    CalendarResourceStatus Status,
    string? RestrictionReason,
    string? UnavailabilityReason,
    CalendarResourceType ResourceType,
    BranchId? ResourceBranchId);

public sealed record ExistingBookingResourceReservation(
    BookingId BookingId,
    CalendarResourceId ResourceId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    int Quantity,
    BookingStatus Status,
    BranchId? BranchId);
