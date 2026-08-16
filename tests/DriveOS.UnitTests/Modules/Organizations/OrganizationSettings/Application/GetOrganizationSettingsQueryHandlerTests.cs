using DriveOS.Modules.Organizations.Application.OrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

public sealed class GetOrganizationSettingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProjectionDoesNotExist()
    {
        var handler = new GetOrganizationSettingsQueryHandler(new EmptyReadService());

        var result = await handler.Handle(
            new GetOrganizationSettingsQuery(OrganizationId.New()),
            CancellationToken.None
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.NotFound, result.Error);
    }

    private sealed class EmptyReadService : IOrganizationSettingsReadService
    {
        public Task<OrganizationSettingsResponse?> GetByOrganizationIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<OrganizationSettingsResponse?>(null);
    }
}
