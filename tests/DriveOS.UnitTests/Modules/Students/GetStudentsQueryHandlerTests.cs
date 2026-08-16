using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Students.Application.Students.GetStudents;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class GetStudentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesTenantScopedFiltersToReadService()
    {
        var readService = new FakeStudentReadService();
        var handler = new GetStudentsQueryHandler(readService);
        var query = new GetStudentsQuery(
            OrganizationId.New(),
            2,
            25,
            "Ada",
            BranchId.New(),
            StudentStatus.Active,
            null,
            StudentSortField.Name,
            SortDirection.Ascending
        );

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(query, readService.LastQuery);
        Assert.Equal(1, result.Value.TotalCount);
    }

    private sealed class FakeStudentReadService : IStudentReadService
    {
        public GetStudentsQuery? LastQuery { get; private set; }

        public Task<PagedResult<StudentListItem>> GetPageAsync(
            GetStudentsQuery query,
            CancellationToken cancellationToken = default
        )
        {
            LastQuery = query;
            StudentListItem item = new(
                Guid.NewGuid(),
                "Ada",
                "Lovelace",
                null,
                null,
                StudentStatus.Active,
                null,
                null,
                null,
                null,
                DateTimeOffset.UtcNow
            );
            return Task.FromResult(
                new PagedResult<StudentListItem>([item], query.PageNumber, query.PageSize, 1)
            );
        }
    }
}
