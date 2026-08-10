using DriveOS.Modules.CRM.Domain.Leads;

namespace DriveOS.Modules.CRM.Application.Leads.GetLeads;

public sealed record LeadListItem(
    Guid Id,
    Guid? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    LeadSourceType SourceType,
    Guid? AssignedAdvisorId,
    LeadStatus Status,
    DateTimeOffset CreatedAtUtc);
