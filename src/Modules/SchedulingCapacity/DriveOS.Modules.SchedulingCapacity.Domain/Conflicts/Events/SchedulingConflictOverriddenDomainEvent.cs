using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts.Events;

public sealed record SchedulingConflictOverriddenDomainEvent(
    SchedulingConflictId ConflictId,
    OrganizationId OrganizationId,
    BookingId BookingId,
    UserId ApprovedByUserId,
    DateTimeOffset ExpiresAtUtc) : DomainEvent;
