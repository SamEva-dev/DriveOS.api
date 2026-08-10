using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Leads.Events;

public sealed record LeadCreatedDomainEvent(
    LeadId LeadId,
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string FirstName,
    string LastName,
    string LicenseCategory,
    LeadSourceType Source)
    : DomainEvent;
