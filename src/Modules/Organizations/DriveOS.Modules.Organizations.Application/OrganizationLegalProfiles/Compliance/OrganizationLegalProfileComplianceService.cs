using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

internal sealed class OrganizationLegalProfileComplianceService(
    IOrganizationLegalProfileCountryRulesProvider rulesProvider)
    : IOrganizationLegalProfileComplianceService
{
    public OrganizationLegalProfileComplianceResult Validate(
        OrganizationLegalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        IOrganizationLegalProfileCountryRules rules =
            rulesProvider.GetRules(profile.RegisteredAddress.CountryCode);

        return rules.Validate(new OrganizationLegalProfileComplianceInput(
            profile.RegisteredAddress.CountryCode,
            profile.LegalForm,
            profile.RegistrationNumber,
            profile.TaxNumber,
            profile.IncorporationDate,
            profile.RegisteredAddress.Line1,
            profile.RegisteredAddress.PostalCode,
            profile.RegisteredAddress.City));
    }
}
