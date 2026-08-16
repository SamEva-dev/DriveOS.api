using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Leads.GetLeads;

public sealed record GetLeadsQuery(
    OrganizationId OrganizationId,
    int PageNumber,
    int PageSize,
    string? Search,
    BranchId? BranchId,
    LeadStatus? Status,
    LeadSourceType? SourceType,
    UserId? AssignedAdvisorId,
    bool UnassignedOnly,
    LeadSortField SortBy,
    SortDirection SortDirection
) : IQuery<PagedResult<LeadListItem>>;
