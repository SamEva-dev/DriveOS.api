using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Abstractions;

public interface IOrganizationReadService
{
    Task<OrganizationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<OrganizationListItem>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        OrganizationSortField sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<OrganizationStatusHistoryItem>> GetStatusHistoryAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken
    );
}
