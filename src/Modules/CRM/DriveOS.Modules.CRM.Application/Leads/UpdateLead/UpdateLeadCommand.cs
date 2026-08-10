using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.UpdateLead;

public sealed record UpdateLeadCommand(
    OrganizationId OrganizationId,
    LeadId LeadId,
    BranchId? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    string? PreferredLocation,
    LeadSourceType SourceType,
    string? SourceDetail) : ICommand;
