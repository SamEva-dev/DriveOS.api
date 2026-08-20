using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingVehicleReplacedDomainEvent(BookingId BookingId, OrganizationId OrganizationId, Guid OperationId,
    Guid PreviousVehicleId, Guid ReplacementVehicleId, CalendarResourceId PreviousResourceId, CalendarResourceId ReplacementResourceId) : DomainEvent;
