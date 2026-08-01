using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain
    .BranchAssignments.Events;

public sealed record
    BranchUserAssignmentReactivatedDomainEvent(
        BranchUserAssignmentId AssignmentId,
        OrganizationId OrganizationId,
        BranchId BranchId,
        UserId UserId,
        string Reason,
        UserId ReactivatedByUserId,
        DateTimeOffset ReactivatedAtUtc)
    : DomainEvent;