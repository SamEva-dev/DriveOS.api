using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;
using Xunit;

namespace DriveOS.Modules.Organizations.Tests.OrganizationLegalProfiles;

public sealed class OrganizationLegalProfileCountryRulesProviderTests
{
    [Fact]
    public void GetRules_ReturnsCountrySpecificRules_WhenRegistered()
    {
        IOrganizationLegalProfileCountryRules[] rules =
        [
            new GenericOrganizationLegalProfileCountryRules(),
            new FranceOrganizationLegalProfileCountryRules()
        ];

        var provider = new OrganizationLegalProfileCountryRulesProvider(rules);

        Assert.Equal("FR", provider.GetRules("fr").CountryCode);
    }

    [Fact]
    public void GetRules_ReturnsGenericRules_ForUnknownCountry()
    {
        IOrganizationLegalProfileCountryRules[] rules =
        [
            new GenericOrganizationLegalProfileCountryRules(),
            new FranceOrganizationLegalProfileCountryRules()
        ];

        var provider = new OrganizationLegalProfileCountryRulesProvider(rules);

        Assert.Equal("*", provider.GetRules("CM").CountryCode);
    }
}
