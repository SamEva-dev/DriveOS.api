using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts.Events;

public sealed record SchedulingConflictDetectedDomainEvent(
    SchedulingConflictId ConflictId,
    OrganizationId OrganizationId,
    BookingId BookingId,
    SchedulingConflictType Type,
    SchedulingConflictPriority Priority) : DomainEvent;
