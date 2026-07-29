using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches.Events;

public sealed record BranchPrimaryDesignationRemovedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId)
    : DomainEvent;
