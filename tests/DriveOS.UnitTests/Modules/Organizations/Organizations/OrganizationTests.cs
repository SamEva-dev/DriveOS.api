using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDraftOrganization()
    {
        OrganizationId id = OrganizationId.New();

        var result = Organization.Create(
            id,
            "Auto-école Horizon",
            "fr",
            OrganizationType.DrivingSchool
        );

        Assert.True(result.IsSuccess);

        Organization organization = result.Value;

        Assert.Equal(id, organization.Id);
        Assert.Equal("Auto-école Horizon", organization.LegalName);
        Assert.Equal("FR", organization.CountryCode);
        Assert.Equal(OrganizationType.DrivingSchool, organization.Type);
        Assert.Equal(OrganizationStatus.Draft, organization.Status);

        Assert.Single(organization.DomainEvents);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Organization.Create(
            OrganizationId.New(),
            " ",
            "FR",
            OrganizationType.DrivingSchool
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.EmptyLegalName, result.Error);
    }

    [Fact]
    public void SetProvisioningIdentity_WithValidValues_ShouldPersistStableIdentity()
    {
        Organization organization = Organization.Create(
            OrganizationId.New(),
            "Auto-école Horizon",
            "FR",
            OrganizationType.DrivingSchool
        ).Value;
        var externalUserId = new UserId(Guid.NewGuid());

        var result = organization.SetProvisioningIdentity(
            externalUserId,
            "authgate-org-stable-key"
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(externalUserId, organization.ProvisioningExternalUserId);
        Assert.Equal("authgate-org-stable-key", organization.ProvisioningKey);
    }

    [Fact]
    public void SetProvisioningIdentity_WithDifferentSecondKey_ShouldFail()
    {
        Organization organization = Organization.Create(
            OrganizationId.New(),
            "Auto-école Horizon",
            "FR",
            OrganizationType.DrivingSchool
        ).Value;
        var externalUserId = new UserId(Guid.NewGuid());
        organization.SetProvisioningIdentity(externalUserId, "first-key");

        var result = organization.SetProvisioningIdentity(externalUserId, "second-key");

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.ProvisioningIdentityAlreadySet, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("F")]
    [InlineData("FRA")]
    [InlineData("F1")]
    public void Create_WithInvalidCountryCode_ShouldFail(string countryCode)
    {
        var result = Organization.Create(
            OrganizationId.New(),
            "Auto-école Horizon",
            countryCode,
            OrganizationType.DrivingSchool
        );

        Assert.True(result.IsFailure);
        Assert.Equal(OrganizationErrors.InvalidCountryCode, result.Error);
    }
}
