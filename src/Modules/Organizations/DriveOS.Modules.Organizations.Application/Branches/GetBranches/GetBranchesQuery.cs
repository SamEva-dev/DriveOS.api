using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.GetBranches;

public sealed record GetBranchesQuery(
    OrganizationId OrganizationId,
    int PageNumber,
    int PageSize,
    string? Search,
    BranchSortField SortBy,
    SortDirection SortDirection
) : IQuery<PagedResult<BranchListItem>>;
