using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingCancellationNotificationRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingCancellationId CancellationId,
    BookingNotificationDecision Decision,
    IReadOnlyCollection<Guid> ParticipantIds) : DomainEvent;
