using System.Text.RegularExpressions;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;

public sealed partial class FranceOrganizationLegalProfileCountryRules : IOrganizationLegalProfileCountryRules
{
    public string CountryCode => "FR";

    public OrganizationLegalProfileComplianceResult Validate(
        OrganizationLegalProfileComplianceInput input)
    {
        var issues = new List<OrganizationLegalProfileComplianceIssue>();
        string registration = Normalize(input.RegistrationNumber);

        if (!FrenchRegistrationNumberRegex().IsMatch(registration))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.FR.RegistrationNumber.Invalid",
                "For France, the registration number must contain 9 or 14 digits.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        if (!string.IsNullOrWhiteSpace(input.TaxNumber) &&
            !FrenchVatNumberRegex().IsMatch(Normalize(input.TaxNumber)))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.FR.TaxNumber.Invalid",
                "The French VAT number format is invalid.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        if (!FrenchPostalCodeRegex().IsMatch(input.PostalCode.Trim()))
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.FR.PostalCode.Invalid",
                "The French postal code must contain five digits.",
                OrganizationLegalProfileComplianceSeverity.Blocking));
        }

        if (input.IncorporationDate is null)
        {
            issues.Add(new(
                "OrganizationLegalProfiles.Compliance.FR.IncorporationDate.Recommended",
                "The incorporation date should be completed for the French legal profile.",
                OrganizationLegalProfileComplianceSeverity.Warning));
        }

        return new OrganizationLegalProfileComplianceResult(issues);
    }

    private static string Normalize(string value) =>
        string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();

    [GeneratedRegex("^(?:[0-9]{9}|[0-9]{14})$", RegexOptions.CultureInvariant)]
    private static partial Regex FrenchRegistrationNumberRegex();

    [GeneratedRegex("^FR[A-Z0-9]{2}[0-9]{9}$", RegexOptions.CultureInvariant)]
    private static partial Regex FrenchVatNumberRegex();

    [GeneratedRegex("^[0-9]{5}$", RegexOptions.CultureInvariant)]
    private static partial Regex FrenchPostalCodeRegex();
}
