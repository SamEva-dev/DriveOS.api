using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.ExportLeads;

public sealed record ExportLeadsQuery(
    OrganizationId OrganizationId, string? Search, BranchId? BranchId, LeadStatus? Status,
    LeadSourceType? SourceType, UserId? AssignedAdvisorId, bool UnassignedOnly)
    : IQuery<LeadExportFile>;

public sealed record LeadExportFile(byte[] Content, string FileName, int ExportedCount);
