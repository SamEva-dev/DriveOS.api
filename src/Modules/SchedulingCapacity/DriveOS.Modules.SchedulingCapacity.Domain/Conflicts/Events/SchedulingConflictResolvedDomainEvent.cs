using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts.Events;

public sealed record SchedulingConflictResolvedDomainEvent(
    SchedulingConflictId ConflictId,
    OrganizationId OrganizationId,
    BookingId BookingId,
    SchedulingConflictResolution Resolution) : DomainEvent;
