using DriveOS.Modules.Students.Application.Students.GetStudentOverview;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Students;

public sealed class GetStudentOverviewQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenStudentIsOutsideTheTenantScope()
    {
        var service = new StubOverviewReadService(null);
        var handler = new GetStudentOverviewQueryHandler(service);
        var query = new GetStudentOverviewQuery(
            OrganizationId.New(),
            PersonId.New(),
            new StudentOverviewReadScope(
                true,
                false,
                false,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
        );

        var result = await handler.Handle(query, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(StudentOverviewErrors.NotFound);
        service.LastQuery.Should().Be(query);
    }

    [Fact]
    public async Task Handle_ShouldReturnTheAuthorizedPartialOverview()
    {
        var response = new StudentOverviewResponse(
            new StudentProfileSummary(
                Guid.NewGuid(),
                "Ada",
                "Lovelace",
                null,
                null,
                StudentStatus.Active,
                DateTimeOffset.UtcNow
            ),
            null,
            [],
            [],
            [],
            []
        );
        var handler = new GetStudentOverviewQueryHandler(new StubOverviewReadService(response));
        var query = new GetStudentOverviewQuery(
            OrganizationId.New(),
            PersonId.New(),
            new StudentOverviewReadScope(
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false
            )
        );

        var result = await handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(response);
    }

    private sealed class StubOverviewReadService(StudentOverviewResponse? response)
        : IStudentOverviewReadService
    {
        public GetStudentOverviewQuery? LastQuery { get; private set; }

        public Task<StudentOverviewResponse?> GetAsync(
            GetStudentOverviewQuery query,
            CancellationToken cancellationToken = default
        )
        {
            LastQuery = query;
            return Task.FromResult(response);
        }
    }
}
