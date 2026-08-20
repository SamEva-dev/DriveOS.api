using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingNotificationRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingNotificationPolicy Policy,
    IReadOnlyCollection<Guid> ParticipantIds) : DomainEvent;
