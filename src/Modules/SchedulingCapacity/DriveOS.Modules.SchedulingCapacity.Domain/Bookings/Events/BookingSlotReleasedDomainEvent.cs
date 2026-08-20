using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingSlotReleasedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    IReadOnlyCollection<CalendarResourceId> ResourceIds) : DomainEvent;
