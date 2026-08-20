using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingCreatedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingType BookingType,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc) : DomainEvent;
