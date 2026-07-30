using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain
    .Branches.Events;

public sealed record BranchManagerAssignmentEndedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId,
    BranchManagerAssignmentId AssignmentId,
    UserId ManagerUserId,
    DateTimeOffset EffectiveToUtc,
    UserId EndedByUserId,
    DateTimeOffset EndedAtUtc)
    : DomainEvent;