using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Compliance;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationLegalProfiles.Compliance;

public sealed class OrganizationLegalProfileCountryRulesProvider(
    IEnumerable<IOrganizationLegalProfileCountryRules> rules
) : IOrganizationLegalProfileCountryRulesProvider
{
    private readonly IReadOnlyDictionary<string, IOrganizationLegalProfileCountryRules> _rules =
        rules.ToDictionary(
            x => x.CountryCode.Trim().ToUpperInvariant(),
            StringComparer.OrdinalIgnoreCase
        );

    public IOrganizationLegalProfileCountryRules GetRules(string countryCode)
    {
        string normalized = countryCode.Trim().ToUpperInvariant();

        if (_rules.TryGetValue(normalized, out IOrganizationLegalProfileCountryRules? countryRules))
            return countryRules;

        if (_rules.TryGetValue("*", out IOrganizationLegalProfileCountryRules? genericRules))
            return genericRules;

        throw new InvalidOperationException(
            "Generic organization legal-profile rules are not registered."
        );
    }
}
