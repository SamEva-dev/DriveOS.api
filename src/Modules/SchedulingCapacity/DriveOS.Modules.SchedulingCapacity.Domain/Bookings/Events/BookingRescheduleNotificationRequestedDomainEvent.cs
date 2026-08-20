using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingRescheduleNotificationRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    Guid OperationId,
    DateTimeOffset PreviousStartAtUtc,
    DateTimeOffset PreviousEndAtUtc,
    DateTimeOffset NewStartAtUtc,
    DateTimeOffset NewEndAtUtc,
    IReadOnlyCollection<Guid> ParticipantIds) : DomainEvent;
