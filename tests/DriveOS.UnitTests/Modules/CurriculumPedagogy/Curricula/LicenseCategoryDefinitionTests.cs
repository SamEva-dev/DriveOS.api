using DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories;
using DriveOS.Modules.CurriculumPedagogy.Domain.LicenseCategories.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.Curricula;

public sealed class LicenseCategoryDefinitionTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly UserId ActorUserId = new(Guid.NewGuid());

    [Fact]
    public void Create_NormalizesCountryAndCategoryAndStartsDraft()
    {
        var result = LicenseCategoryDefinition.Create(
            LicenseCategoryDefinitionId.New(),
            OrganizationId,
            " fr ",
            " b ",
            " Permis B ",
            " Véhicules légers. ");

        result.IsSuccess.Should().BeTrue();
        result.Value.CountryCode.Should().Be("FR");
        result.Value.Code.Should().Be("B");
        result.Value.Name.Should().Be("Permis B");
        result.Value.Status.Should().Be(LicenseCategoryDefinitionStatus.Draft);
        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<LicenseCategoryDefinitionCreatedDomainEvent>();
    }

    [Theory]
    [InlineData("FRA", "B")]
    [InlineData("FR", "B B")]
    [InlineData("FR", "")]
    public void Create_WithInvalidScope_Fails(string countryCode, string categoryCode)
    {
        var result = LicenseCategoryDefinition.Create(
            LicenseCategoryDefinitionId.New(),
            OrganizationId,
            countryCode,
            categoryCode,
            "Permis",
            null);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Activate_FreezesMetadata()
    {
        LicenseCategoryDefinition definition = CreateDefinition();

        definition.Activate(ActorUserId, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        definition.Status.Should().Be(LicenseCategoryDefinitionStatus.Active);
        definition.DomainEvents.Should().Contain(e => e is LicenseCategoryDefinitionActivatedDomainEvent);

        var update = definition.UpdateMetadata("Nouveau nom", null);
        update.IsFailure.Should().BeTrue();
        update.Error.Should().Be(LicenseCategoryDefinitionErrors.ModificationNotAllowed);
    }

    [Fact]
    public void Matches_UsesNormalizedScope()
    {
        LicenseCategoryDefinition definition = CreateDefinition();

        definition.Matches("fr", "b").Should().BeTrue();
        definition.Matches("BE", "B").Should().BeFalse();
        definition.Matches("FR", "A2").Should().BeFalse();
    }

    [Fact]
    public void Archive_PreservesDefinitionButMakesItUnavailableForNewUse()
    {
        LicenseCategoryDefinition definition = CreateDefinition();
        definition.Activate(ActorUserId, DateTimeOffset.UtcNow);

        var result = definition.Archive(ActorUserId, DateTimeOffset.UtcNow.AddMinutes(1));

        result.IsSuccess.Should().BeTrue();
        definition.Status.Should().Be(LicenseCategoryDefinitionStatus.Archived);
        definition.DomainEvents.Should().Contain(e => e is LicenseCategoryDefinitionArchivedDomainEvent);
    }

    private static LicenseCategoryDefinition CreateDefinition() =>
        LicenseCategoryDefinition.Create(
            LicenseCategoryDefinitionId.New(),
            OrganizationId,
            "FR",
            "B",
            "Permis B",
            "Véhicules légers").Value;
}
