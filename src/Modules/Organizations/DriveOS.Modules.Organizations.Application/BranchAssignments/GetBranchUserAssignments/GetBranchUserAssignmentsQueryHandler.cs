using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Modules.Organizations.Application.BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.GetBranchUserAssignments;

internal sealed class GetBranchUserAssignmentsQueryHandler(
    IBranchRepository branchRepository,
    IBranchUserAssignmentReadService readService
) : IQueryHandler<GetBranchUserAssignmentsQuery, PagedResult<BranchUserAssignmentItem>>
{
    public async Task<Result<PagedResult<BranchUserAssignmentItem>>> Handle(
        GetBranchUserAssignmentsQuery query,
        CancellationToken cancellationToken
    )
    {
        Branch? branch = await branchRepository.GetByIdAsync(
            query.BranchId,
            asNoTracking: true,
            cancellationToken
        );

        if (branch is null || branch.OrganizationId != query.OrganizationId)
        {
            return Result.Failure<PagedResult<BranchUserAssignmentItem>>(
                BranchUserAssignmentErrors.BranchNotFound
            );
        }

        PagedResult<BranchUserAssignmentItem> result = await readService.GetByBranchAsync(
            query.OrganizationId,
            query.BranchId,
            query.PageNumber,
            query.PageSize,
            query.Search,
            query.Status,
            query.Role,
            query.AssignmentType,
            query.SortBy,
            query.SortDirection,
            cancellationToken
        );

        return Result.Success(result);
    }
}
