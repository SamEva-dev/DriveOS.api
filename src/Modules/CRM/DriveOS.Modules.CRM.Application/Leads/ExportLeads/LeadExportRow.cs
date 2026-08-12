using DriveOS.Modules.CRM.Domain.Leads;

namespace DriveOS.Modules.CRM.Application.Leads.ExportLeads;

public sealed record LeadExportRow(
    Guid Id, string FirstName, string LastName, string? Email, string? Phone,
    string LicenseCategory, TransmissionPreference Transmission, LeadSourceType SourceType,
    Guid? BranchId, Guid? AssignedAdvisorId, LeadStatus Status, DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);
