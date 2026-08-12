using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record LeadQualifiedDomainEvent(LeadId LeadId, OrganizationId OrganizationId)
    : DomainEvent;
