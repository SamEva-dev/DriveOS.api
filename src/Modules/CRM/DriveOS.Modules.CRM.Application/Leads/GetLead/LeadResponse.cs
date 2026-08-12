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
    LeadQualificationResponse? Qualification,
    Guid? ConvertedPersonId,
    Guid? DraftEnrollmentId,
    DateTimeOffset? ConvertedAtUtc,
    LeadClosureReason? ClosureReason,
    string? ClosureComment,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset? ResumeAtUtc,
    Guid? DormancyResponsibleUserId,
    string? DormancyCampaignCode,
    string? ReferredPartnerName,
    string? SharedDataDescription,
    DateTimeOffset? ReferralConsentCollectedAtUtc,
    DateTimeOffset? ReopenedAtUtc,
    bool AutomaticFollowUpsEnabled,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId);

public sealed record LeadQualificationResponse(string Need, string LicenseCategory,
    string Availability, DateOnly? TargetDate, FinancingOption Financing, string? Notes);
