using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches.Events;

public sealed record BranchManagerAssignedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId,
    BranchManagerAssignmentId AssignmentId,
    UserId ManagerUserId,
    DateTimeOffset EffectiveFromUtc,
    UserId AssignedByUserId,
    DateTimeOffset AssignedAtUtc
) : DomainEvent;
