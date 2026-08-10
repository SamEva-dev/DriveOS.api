using DriveOS.Modules.CRM.Domain.Leads;

namespace DriveOS.Modules.CRM.Application.Leads.GetLead;

public sealed record LeadResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    string? PreferredLocation,
    LeadSourceType SourceType,
    string? SourceDetail,
    Guid? AssignedAdvisorId,
    LeadStatus Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId);
