using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches.Events;

public sealed record BranchCreatedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId,
    string Name,
    string Code,
    BranchType BranchType,
    bool IsPrimary)
    : DomainEvent;
