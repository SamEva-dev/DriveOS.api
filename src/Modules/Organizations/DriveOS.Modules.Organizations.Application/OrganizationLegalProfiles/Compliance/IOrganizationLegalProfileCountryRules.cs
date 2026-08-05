namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

public interface IOrganizationLegalProfileCountryRules
{
    string CountryCode { get; }

    OrganizationLegalProfileComplianceResult Validate(
        OrganizationLegalProfileComplianceInput input);
}
