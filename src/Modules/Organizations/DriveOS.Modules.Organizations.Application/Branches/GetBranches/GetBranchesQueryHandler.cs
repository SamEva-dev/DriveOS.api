using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.SharedKernel.Results;


namespace DriveOS.Modules.Organizations.Application.Branches.GetBranches;

public sealed class GetBranchesQueryHandler(
    IBranchReadService branchReadService)
    : IQueryHandler<GetBranchesQuery, PagedResult<BranchListItem>>
{
    public async Task<Result<PagedResult<BranchListItem>>> Handle(
        GetBranchesQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<BranchListItem> result =
            await branchReadService.GetPagedAsync(
                query.OrganizationId,
                query.PageNumber,
                query.PageSize,
                query.Search,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

        return Result.Success(result);
    }
}
