using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Modules.Organizations.Application
    .Branches.Models;
using DriveOS.Modules.Organizations.Application
    .Branches.StatusHistory;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.Application.Abstractions.Sorting;

namespace DriveOS.Modules.Organizations.Application
    .Branches;

public interface IBranchReadService
{
    Task<BranchResponse?> GetByIdAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken);

    Task<PagedResult<BranchListItem>>
        GetPagedAsync(
            OrganizationId organizationId,
            int pageNumber,
            int pageSize,
            string? search,
            BranchSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken);

    Task<IReadOnlyList<BranchStatusHistoryItem>>
        GetStatusHistoryAsync(
            OrganizationId organizationId,
            BranchId branchId,
            CancellationToken cancellationToken);
}