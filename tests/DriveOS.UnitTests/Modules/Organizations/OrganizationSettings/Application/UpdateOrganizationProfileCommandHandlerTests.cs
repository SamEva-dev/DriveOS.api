using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.UnitTests.OrganizationSettings;

namespace DriveOS.UnitTests.OrganizationSettings.Application;

public sealed class UpdateOrganizationProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateAggregateAndCommit_WhenVersionMatches()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        var repository = new FakeOrganizationSettingsRepository { Settings = settings };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateOrganizationProfileCommandHandler(repository, unitOfWork);

        var command = new UpdateOrganizationProfileCommand(
            settings.OrganizationId,
            "Nouvelle enseigne",
            "RCS-42",
            "FR42",
            settings.Version
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nouvelle enseigne", settings.Profile.TradeName);
        Assert.Equal(2, settings.Version);
        Assert.Equal(1, unitOfWork.CommitCallCount);
    }

    [Fact]
    public async Task Handle_ShouldFailWithoutCommit_WhenVersionIsStale()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        var repository = new FakeOrganizationSettingsRepository { Settings = settings };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateOrganizationProfileCommandHandler(repository, unitOfWork);

        var command = new UpdateOrganizationProfileCommand(
            settings.OrganizationId,
            "Nouvelle enseigne",
            null,
            null,
            settings.Version + 1
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationSettingsErrors.ConcurrentUpdate, result.Error);
        Assert.Equal(0, unitOfWork.CommitCallCount);
    }
}
