using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations.Fakes;

internal sealed class FakeOrganizationReadService
    : IOrganizationReadService
{
    public OrganizationResponse?
        OrganizationByIdResult
    { get; set; }

    public PagedResult<OrganizationListItem>
        PagedResult
    { get; set; } =
            new(
                [],
                PageNumber: 1,
                PageSize: 20,
                TotalCount: 0);

    public IReadOnlyList<OrganizationStatusHistoryItem>
        StatusHistoryResult
    { get; set; } = [];

    public OrganizationId?
        LastRequestedOrganizationId
    { get; private set; }

    public int?
        LastPageNumber
    { get; private set; }

    public int?
        LastPageSize
    { get; private set; }

    public string?
        LastSearch
    { get; private set; }

    public OrganizationSortField?
        LastSortBy
    { get; private set; }

    public SortDirection?
        LastSortDirection
    { get; private set; }

    public Task<OrganizationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        LastRequestedOrganizationId =
            organizationId;

        return Task.FromResult(
            OrganizationByIdResult);
    }

    public Task<PagedResult<OrganizationListItem>>
        GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            OrganizationSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default)
    {
        LastPageNumber = pageNumber;
        LastPageSize = pageSize;
        LastSearch = search;
        LastSortBy = sortBy;
        LastSortDirection = sortDirection;

        return Task.FromResult(
            PagedResult);
    }

    public Task<IReadOnlyList<OrganizationStatusHistoryItem>> GetStatusHistoryAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        LastRequestedOrganizationId = organizationId;
        return Task.FromResult(StatusHistoryResult);
    }
}