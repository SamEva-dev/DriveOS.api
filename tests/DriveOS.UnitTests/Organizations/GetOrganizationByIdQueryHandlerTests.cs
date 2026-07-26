using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations;

public sealed class GetOrganizationByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrganizationExists_ShouldReturnIt()
    {
        OrganizationId id = OrganizationId.New();

        var expected = new OrganizationResponse(
            id.Value,
            "Auto-école Horizon",
            "FR",
            "DrivingSchool",
            "Draft",
            DateTimeOffset.UtcNow,
            null,
            null,
            null);

        var readService =
            new FakeOrganizationReadService(expected);

        var handler =
            new GetOrganizationByIdQueryHandler(
                readService);

        var query =
            new GetOrganizationByIdQuery(id);

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_ShouldFail()
    {
        var readService =
            new FakeOrganizationReadService(null);

        var handler =
            new GetOrganizationByIdQueryHandler(
                readService);

        var query =
            new GetOrganizationByIdQuery(
                OrganizationId.New());

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(
            OrganizationErrors.NotFound,
            result.Error);
    }

    private sealed class FakeOrganizationReadService
        : IOrganizationReadService
    {
        private readonly OrganizationResponse?
            _organization;

        public FakeOrganizationReadService(
            OrganizationResponse? organization)
        {
            _organization = organization;
        }

        public Task<OrganizationResponse?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_organization);
        }
    }
}