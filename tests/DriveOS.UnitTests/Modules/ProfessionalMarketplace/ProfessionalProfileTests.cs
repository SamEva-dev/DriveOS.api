using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalProfileTests
{
    private static readonly UserId Actor = new(Guid.NewGuid());
    private static ProfessionalProfile NewProfile() => ProfessionalProfile.Create(ProfessionalProfileId.New(), new PersonId(Guid.NewGuid()), new OrganizationId(Guid.NewGuid()), null, DateTimeOffset.UtcNow).Value;

    [Fact]
    public void Create_separates_person_from_provider_organization()
    {
        var p = NewProfile();
        Assert.NotEqual(p.PersonId.Value, p.ProviderOrganizationId.Value);
        Assert.Equal(ProfessionalProfileStatus.Draft, p.Status);
    }

    [Fact]
    public void Complete_requires_business_presentation_categories_languages_and_service_area()
    {
        var p = NewProfile();
        var result = p.CompleteProfile(DateTimeOffset.UtcNow, Actor);
        Assert.True(result.IsFailure);
        Assert.Equal(ProfessionalProfileErrors.ProfileIncomplete, result.Error);
        Assert.Equal(ProfessionalProfileStatus.Incomplete, p.Status);
    }

    [Fact]
    public void Complete_succeeds_when_required_profile_data_is_present()
    {
        var p = NewProfile();
        Assert.True(p.UpdateBusinessIdentity(ProfessionalType.DrivingInstructor,"Jean Dupont EI",null,"EI","SIRET-1",null,"jean@example.test",null,"1 rue Test",null,"06000","Nice","FR",DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.True(p.UpdatePresentation("Enseignant indépendant",null,8,["fr"],["B"],["AAC"],DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.True(p.ReplaceTeachingCapabilities(
            [new TeachingCapability("B", ["IN_CAR"], ["ADULT"], ["FR"], ["AAC"])],
            DateTimeOffset.UtcNow,
            Actor).IsSuccess);
        Assert.True(p.ReplaceServiceAreas(
            [new ProfessionalServiceArea("NICE", "FR", "Nice", null, null, 25, true, ProfessionalMobilityMode.Radius)],
            DateTimeOffset.UtcNow,
            Actor).IsSuccess);
        Assert.True(p.ReplaceEngagementPreferences([ProfessionalEngagementType.HourlyService],DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.True(p.CompleteProfile(DateTimeOffset.UtcNow,Actor).IsSuccess);
        Assert.Equal(ProfessionalProfileStatus.PendingVerification,p.Status);
        Assert.True(p.IsProfileComplete);
    }

    [Fact]
    public void Changing_teaching_categories_invalidates_compliance()
    {
        var p = NewProfile();
        p.UpdatePresentation("Enseignant",null,2,["FR"],["B"],null,DateTimeOffset.UtcNow,Actor);
        p.MarkCompliance(ProfessionalComplianceStatus.Compliant,DateTimeOffset.UtcNow,Actor);
        p.UpdatePresentation("Enseignant",null,2,["FR"],["A2"],null,DateTimeOffset.UtcNow,Actor);
        Assert.Equal(ProfessionalComplianceStatus.Incomplete,p.ComplianceStatus);
        Assert.Equal(ProfessionalProfileStatus.Incomplete,p.Status);
    }

    [Fact]
    public void Activate_requires_complete_and_compliant_profile()
    {
        var p = NewProfile();
        var activation = p.Activate(DateTimeOffset.UtcNow, Actor);
        Assert.True(activation.IsFailure);
        Assert.Equal(ProfessionalProfileErrors.ProfileIncomplete, activation.Error);
    }

    [Fact]
    public void Teaching_capabilities_must_cover_every_declared_category()
    {
        var p = NewProfile();
        p.UpdatePresentation("Instructor", null, 5, ["FR"], ["B", "A2"], [], DateTimeOffset.UtcNow, Actor);

        var result = p.ReplaceTeachingCapabilities(
            [new TeachingCapability("B", ["IN_CAR"], ["ADULT"], ["FR"], [])],
            DateTimeOffset.UtcNow,
            Actor);

        Assert.True(result.IsFailure);
        Assert.Equal("ProfessionalMarketplace.Profile.InvalidTeachingCapabilities", result.Error.Code);
    }

    [Fact]
    public void Changing_teaching_capabilities_invalidates_previous_compliance()
    {
        var p = NewProfile();
        p.UpdatePresentation("Instructor", null, 5, ["FR"], ["B"], ["AAC"], DateTimeOffset.UtcNow, Actor);
        p.MarkCompliance(ProfessionalComplianceStatus.Compliant, DateTimeOffset.UtcNow, Actor);

        var result = p.ReplaceTeachingCapabilities(
            [new TeachingCapability("B", ["IN_CAR"], ["ADULT"], ["FR"], ["AAC"])],
            DateTimeOffset.UtcNow,
            Actor);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProfessionalComplianceStatus.Incomplete, p.ComplianceStatus);
        Assert.Equal(ProfessionalProfileStatus.Incomplete, p.Status);
    }

}
