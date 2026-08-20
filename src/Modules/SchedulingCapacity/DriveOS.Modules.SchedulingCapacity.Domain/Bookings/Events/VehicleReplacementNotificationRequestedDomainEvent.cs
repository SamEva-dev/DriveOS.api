using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record VehicleReplacementNotificationRequestedDomainEvent(BookingId BookingId, OrganizationId OrganizationId, Guid OperationId,
    Guid PreviousVehicleId, Guid ReplacementVehicleId, IReadOnlyCollection<Guid> ParticipantIds) : DomainEvent;
