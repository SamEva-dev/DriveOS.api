using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.CRM.Application.Leads.GetLead;
using DriveOS.Modules.CRM.Application.Leads.GetLeads;
using DriveOS.Modules.CRM.Application.Leads.ExportLeads;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Abstractions.Persistence;

public interface ILeadReadService
{
    Task<LeadResponse?> GetByIdAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<LeadListItem>> GetPagedAsync(
        OrganizationId organizationId,
        int pageNumber,
        int pageSize,
        string? search,
        BranchId? branchId,
        LeadStatus? status,
        LeadSourceType? sourceType,
        UserId? assignedAdvisorId,
        bool unassignedOnly,
        LeadSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeadExportRow>> GetForExportAsync(
        OrganizationId organizationId, string? search, BranchId? branchId,
        LeadStatus? status, LeadSourceType? sourceType, UserId? assignedAdvisorId,
        bool unassignedOnly, int maximumRows, CancellationToken cancellationToken = default);
}
