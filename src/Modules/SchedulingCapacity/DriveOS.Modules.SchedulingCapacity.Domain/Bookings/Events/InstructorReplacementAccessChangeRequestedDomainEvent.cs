using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record InstructorReplacementAccessChangeRequestedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    UserId PreviousInstructorId,
    UserId ReplacementInstructorId,
    DateTimeOffset EffectiveAtUtc,
    DateTimeOffset? ReplacementAccessExpiresAtUtc) : DomainEvent;
