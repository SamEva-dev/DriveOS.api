using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record InstructorReplacementNotificationRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    UserId PreviousInstructorId,
    UserId ReplacementInstructorId,
    IReadOnlyCollection<Guid> ParticipantIds) : DomainEvent;
