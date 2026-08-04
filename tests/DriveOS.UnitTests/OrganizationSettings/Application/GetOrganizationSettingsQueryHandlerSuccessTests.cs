using DriveOS.Modules.Organizations.Application.OrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

public sealed class GetOrganizationSettingsQueryHandlerSuccessTests
{
    [Fact]
    public async Task Handle_ShouldReturnProjectionWithoutLoadingAggregate()
    {
        OrganizationId organizationId = OrganizationId.New();
        OrganizationSettingsResponse expected = CreateResponse(organizationId);
        var readService = new CapturingReadService(expected);
        var handler = new GetOrganizationSettingsQueryHandler(readService);

        var result = await handler.Handle(
            new GetOrganizationSettingsQuery(organizationId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
        Assert.Equal(organizationId, readService.RequestedOrganizationId);
    }

    private static OrganizationSettingsResponse CreateResponse(OrganizationId organizationId) =>
        new(
            Guid.NewGuid(),
            organizationId.Value,
            "Auto-école Horizon",
            "RCS-123",
            "FR123",
            "contact@horizon.test",
            "+33400000000",
            "https://horizon.test",
            "10 avenue de France",
            null,
            "06000",
            "Nice",
            "Provence-Alpes-Côte d'Azur",
            "FR",
            "fr-FR",
            ["fr-FR", "en-GB"],
            "Europe/Paris",
            "EUR",
            "dd/MM/yyyy",
            "HH:mm",
            (int)DayOfWeek.Monday,
            0,
            60,
            120,
            24,
            true,
            false,
            null,
            1,
            DateTimeOffset.UtcNow,
            null);

    private sealed class CapturingReadService(OrganizationSettingsResponse response)
        : IOrganizationSettingsReadService
    {
        public OrganizationId? RequestedOrganizationId { get; private set; }

        public Task<OrganizationSettingsResponse?> GetByOrganizationIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default)
        {
            RequestedOrganizationId = organizationId;
            return Task.FromResult<OrganizationSettingsResponse?>(response);
        }
    }
}
