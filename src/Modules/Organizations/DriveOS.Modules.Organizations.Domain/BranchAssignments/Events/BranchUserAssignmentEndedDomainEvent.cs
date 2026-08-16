using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.BranchAssignments.Events;

public sealed record BranchUserAssignmentEndedDomainEvent(
    BranchUserAssignmentId AssignmentId,
    OrganizationId OrganizationId,
    BranchId BranchId,
    UserId UserId,
    string Reason,
    DateTimeOffset EffectiveEndAtUtc,
    UserId EndedByUserId,
    DateTimeOffset EndedAtUtc
) : DomainEvent;
