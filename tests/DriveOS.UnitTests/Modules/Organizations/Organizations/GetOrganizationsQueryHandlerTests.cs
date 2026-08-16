using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.SharedKernel.Results;
using DriveOS.UnitTests.Organizations.Fakes;

namespace DriveOS.UnitTests.Organizations;

public sealed class GetOrganizationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPagedOrganizations()
    {
        var expectedItem = new OrganizationListItem(
            Guid.NewGuid(),
            "Auto-école Horizon",
            "FR",
            "DrivingSchool",
            "Draft",
            DateTimeOffset.UtcNow
        );

        var expectedPage = new PagedResult<OrganizationListItem>(
            [expectedItem],
            PageNumber: 2,
            PageSize: 10,
            TotalCount: 21
        );

        var readService = new FakeOrganizationReadService { PagedResult = expectedPage };

        var handler = new GetOrganizationsQueryHandler(readService);

        var query = new GetOrganizationsQuery(
            PageNumber: 2,
            PageSize: 10,
            Search: "horizon",
            SortBy: OrganizationSortField.CreatedAtUtc,
            SortDirection: SortDirection.Descending
        );

        Result<PagedResult<OrganizationListItem>> result = await handler.Handle(
            query,
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPage, result.Value);

        Assert.Equal(2, readService.LastPageNumber);

        Assert.Equal(10, readService.LastPageSize);

        Assert.Equal("horizon", readService.LastSearch);

        Assert.Equal(OrganizationSortField.CreatedAtUtc, readService.LastSortBy);

        Assert.Equal(SortDirection.Descending, readService.LastSortDirection);
    }
}
