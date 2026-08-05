namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public interface IOrganizationLegalProfileCountryRulesProvider
{
    IOrganizationLegalProfileCountryRules GetRules(string countryCode);
}
