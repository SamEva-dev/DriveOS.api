using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingReservedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId) : DomainEvent;
