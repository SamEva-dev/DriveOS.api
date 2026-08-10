using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.CreateLead;

public sealed record CreateLeadCommand(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    string? PreferredLocation,
    LeadSourceType SourceType,
    string? SourceDetail,
    UserId? AssignedAdvisorId) : ICommand<LeadId>;
