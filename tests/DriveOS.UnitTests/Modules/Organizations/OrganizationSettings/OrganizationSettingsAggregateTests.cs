using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.OrganizationSettings;

public sealed class OrganizationSettingsAggregateTests
{
    [Fact]
    public void Create_ShouldCreateAggregateAndRaiseEvent_WhenValuesAreValid()
    {
        OrganizationId organizationId = OrganizationId.New();

        var result = DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings.Create(
            OrganizationSettingsId.New(),
            organizationId,
            OrganizationSettingsTestData.CreateProfile(),
            OrganizationSettingsTestData.CreateContact(),
            OrganizationSettingsTestData.CreateAddress(),
            OrganizationSettingsTestData.CreateRegional(),
            OrganizationSettingsTestData.CreateOperational());

        result.IsSuccess.Should().BeTrue();
        result.Value.OrganizationId.Should().Be(organizationId);
        result.Value.Version.Should().Be(1);
        result.Value.DomainEvents
            .OfType<OrganizationSettingsCreatedDomainEvent>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void UpdateRegionalSettings_ShouldIncrementVersionAndRaiseEvent_WhenValuesChange()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        OrganizationRegionalSettings regional =
            OrganizationRegionalSettings.Create(
                "en-GB",
                ["fr-FR", "en-GB"],
                "Europe/London",
                "GBP",
                "dd/MM/yyyy",
                "HH:mm",
                DayOfWeek.Monday,
                MeasurementSystem.Metric).Value;

        var result = settings.UpdateRegionalSettings(regional);

        result.IsSuccess.Should().BeTrue();
        settings.Version.Should().Be(2);
        settings.Regional.Should().Be(regional);
        settings.DomainEvents
            .OfType<OrganizationSettingsChangedDomainEvent>()
            .Should()
            .Contain(changed =>
                changed.Section == OrganizationSettingsSection.Regional &&
                changed.Version == 2);
    }

    [Fact]
    public void UpdateProfile_ShouldNotIncrementVersion_WhenValueIsUnchanged()
    {
        var settings = OrganizationSettingsTestData.CreateAggregate();
        int initialEventCount = settings.DomainEvents.Count;

        var result = settings.UpdateProfile(settings.Profile);

        result.IsSuccess.Should().BeTrue();
        settings.Version.Should().Be(1);
        settings.DomainEvents.Should().HaveCount(initialEventCount);
    }

    [Fact]
    public void OperationalSettings_ShouldFail_WhenBranchIsRequiredWithoutDefaultBranch()
    {
        var result = OrganizationOperationalSettings.Create(
            60,
            120,
            24,
            true,
            true,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationSettingsErrors.DefaultBranchRequired);
    }

    [Fact]
    public void RegionalSettings_ShouldFail_WhenDefaultLanguageIsNotSupported()
    {
        var result = OrganizationRegionalSettings.Create(
            "fr-FR",
            ["en-GB"],
            "Europe/Paris",
            "EUR",
            "dd/MM/yyyy",
            "HH:mm",
            DayOfWeek.Monday,
            MeasurementSystem.Metric);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationSettingsErrors.DefaultLanguageNotSupported);
    }
}
