using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record LeadStatusChangedDomainEvent(
    LeadId LeadId,
    OrganizationId OrganizationId,
    LeadStatus PreviousStatus,
    LeadStatus NewStatus,
    string? Reason
) : DomainEvent;
