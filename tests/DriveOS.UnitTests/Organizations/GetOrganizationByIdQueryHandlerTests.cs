using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.UnitTests.Organizations.Fakes;

namespace DriveOS.UnitTests.Organizations;

public sealed class GetOrganizationByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrganizationExists_ShouldReturnIt()
    {
        OrganizationId organizationId =
            OrganizationId.New();

        var expected =
            new OrganizationResponse(
                organizationId.Value,
                "Auto-école Horizon",
                "FR",
                "DrivingSchool",
                "Draft",
                DateTimeOffset.UtcNow,
                null,
                null,
                null);

        var readService =
            new FakeOrganizationReadService
            {
                OrganizationByIdResult =
                    expected
            };

        var handler =
            new GetOrganizationByIdQueryHandler(
                readService);

        var query =
            new GetOrganizationByIdQuery(
                organizationId);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        Assert.Equal(
            organizationId,
            readService
                .LastRequestedOrganizationId);
    }

    [Fact]
    public async Task Handle_WhenOrganizationDoesNotExist_ShouldFail()
    {
        OrganizationId organizationId =
            OrganizationId.New();

        var readService =
            new FakeOrganizationReadService();

        var handler =
            new GetOrganizationByIdQueryHandler(
                readService);

        var query =
            new GetOrganizationByIdQuery(
                organizationId);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            OrganizationErrors.NotFound,
            result.Error);
    }
}