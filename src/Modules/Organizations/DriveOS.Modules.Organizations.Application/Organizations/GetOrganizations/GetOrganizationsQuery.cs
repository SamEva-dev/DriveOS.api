using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;

namespace DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;

public sealed record GetOrganizationsQuery(
    int PageNumber,
    int PageSize,
    string? Search,
    OrganizationSortField SortBy,
    SortDirection SortDirection
) : IQuery<PagedResult<OrganizationListItem>>;
