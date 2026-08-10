using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.GetLeads;

public sealed class GetLeadsQueryHandler(ILeadReadService leadReadService)
    : IQueryHandler<GetLeadsQuery, PagedResult<LeadListItem>>
{
    public async Task<Result<PagedResult<LeadListItem>>> Handle(
        GetLeadsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<LeadListItem> page = await leadReadService.GetPagedAsync(
            query.OrganizationId,
            query.PageNumber,
            query.PageSize,
            query.Search,
            query.BranchId,
            query.Status,
            query.SourceType,
            query.AssignedAdvisorId,
            query.UnassignedOnly,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        return Result.Success(page);
    }
}
