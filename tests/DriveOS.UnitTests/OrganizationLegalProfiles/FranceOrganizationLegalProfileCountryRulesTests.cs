using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;
using Xunit;

namespace DriveOS.Modules.Organizations.Tests.OrganizationLegalProfiles;

public sealed class FranceOrganizationLegalProfileCountryRulesTests
{
    [Fact]
    public void Validate_WithValidFrenchValues_DoesNotReturnBlockingIssue()
    {
        var rules = new FranceOrganizationLegalProfileCountryRules();
        var input = new OrganizationLegalProfileComplianceInput(
            "FR", OrganizationLegalForm.LimitedLiabilityCompany,
            "123 456 789", "FR12123456789", new DateOnly(2024, 1, 1),
            "10 rue de la République", "06000", "Nice");

        OrganizationLegalProfileComplianceResult result = rules.Validate(input);

        Assert.True(result.IsCompliant);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("12345678")]
    [InlineData("1234567890123")]
    public void Validate_WithInvalidRegistrationNumber_ReturnsBlockingIssue(string value)
    {
        var rules = new FranceOrganizationLegalProfileCountryRules();
        var input = new OrganizationLegalProfileComplianceInput(
            "FR", OrganizationLegalForm.LimitedLiabilityCompany,
            value, null, new DateOnly(2024, 1, 1),
            "10 rue de la République", "06000", "Nice");

        OrganizationLegalProfileComplianceResult result = rules.Validate(input);

        Assert.Contains(result.Issues, x =>
            x.Code == "OrganizationLegalProfiles.Compliance.FR.RegistrationNumber.Invalid" &&
            x.Severity == OrganizationLegalProfileComplianceSeverity.Blocking);
    }
}
