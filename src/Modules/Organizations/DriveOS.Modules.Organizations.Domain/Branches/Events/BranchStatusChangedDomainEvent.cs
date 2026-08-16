using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Branches.Events;

public sealed record BranchStatusChangedDomainEvent(
    BranchId BranchId,
    OrganizationId OrganizationId,
    BranchStatus PreviousStatus,
    BranchStatus NewStatus,
    string Reason,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAtUtc
) : DomainEvent;
