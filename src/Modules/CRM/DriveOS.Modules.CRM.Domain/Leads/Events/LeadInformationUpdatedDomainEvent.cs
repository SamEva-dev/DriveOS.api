using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record LeadInformationUpdatedDomainEvent(
    LeadId LeadId,
    OrganizationId OrganizationId,
    BranchId? BranchId)
    : DomainEvent;
