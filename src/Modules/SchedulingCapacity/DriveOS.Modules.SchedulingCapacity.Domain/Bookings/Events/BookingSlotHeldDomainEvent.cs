using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingSlotHeldDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
