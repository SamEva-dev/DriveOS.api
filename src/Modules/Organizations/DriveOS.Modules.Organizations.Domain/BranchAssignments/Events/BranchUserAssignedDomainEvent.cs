using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.BranchAssignments.Events;

public sealed record BranchUserAssignedDomainEvent(
    BranchUserAssignmentId AssignmentId,
    OrganizationId OrganizationId,
    BranchId BranchId,
    UserId UserId,
    BranchAssignmentRole Role,
    BranchAssignmentType AssignmentType,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset? PlannedEndAtUtc,
    UserId AssignedByUserId,
    DateTimeOffset AssignedAtUtc
) : DomainEvent;
