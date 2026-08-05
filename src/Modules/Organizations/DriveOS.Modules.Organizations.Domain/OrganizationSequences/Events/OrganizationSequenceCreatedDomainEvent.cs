using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences.Events;

public sealed record OrganizationSequenceCreatedDomainEvent(
    OrganizationSequenceId SequenceId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string Code) : DomainEvent;
