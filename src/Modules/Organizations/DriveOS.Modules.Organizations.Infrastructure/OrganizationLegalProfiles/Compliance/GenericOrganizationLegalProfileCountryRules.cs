using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;

internal sealed class GenericOrganizationLegalProfileCountryRules : IOrganizationLegalProfileCountryRules
{
    public string CountryCode => "*";

    public OrganizationLegalProfileComplianceResult Validate(
        OrganizationLegalProfileComplianceInput input)
    {
        var issues = new List<OrganizationLegalProfileComplianceIssue>();

        if (string.IsNullOrWhiteSpace(input.RegistrationNumber))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.RegistrationNumber.Required",
                "A registration number is required before activation.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        if (string.IsNullOrWhiteSpace(input.AddressLine1) ||
            string.IsNullOrWhiteSpace(input.PostalCode) ||
            string.IsNullOrWhiteSpace(input.City))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.RegisteredAddress.Incomplete",
                "The registered address is incomplete.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        if (input.IncorporationDate is not null && input.IncorporationDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.IncorporationDate.Future",
                "The incorporation date cannot be in the future.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        return new OrganizationLegalProfileComplianceResult(issues);
    }
}
