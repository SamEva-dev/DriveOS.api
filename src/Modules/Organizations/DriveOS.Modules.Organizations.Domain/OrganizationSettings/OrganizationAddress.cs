using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed record OrganizationAddress
{
    public const int AddressLineMaximumLength = 200;
    public const int PostalCodeMaximumLength = 20;
    public const int CityMaximumLength = 120;
    public const int RegionMaximumLength = 120;

    private OrganizationAddress(
        string? line1,
        string? line2,
        string? postalCode,
        string? city,
        string? region,
        string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
        City = city;
        Region = region;
        CountryCode = countryCode;
    }

    public string? Line1 { get; }
    public string? Line2 { get; }
    public string? PostalCode { get; }
    public string? City { get; }
    public string? Region { get; }
    public string CountryCode { get; }

    public static Result<OrganizationAddress> Create(
        string? line1,
        string? line2,
        string? postalCode,
        string? city,
        string? region,
        string? countryCode)
    {
        string? normalizedLine1 = NormalizeOptional(line1);
        string? normalizedLine2 = NormalizeOptional(line2);
        string? normalizedPostalCode = NormalizeOptional(postalCode);
        string? normalizedCity = NormalizeOptional(city);
        string? normalizedRegion = NormalizeOptional(region);
        string normalizedCountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalizedCountryCode.Length != 2 || !normalizedCountryCode.All(char.IsLetter))
        {
            return Result.Failure<OrganizationAddress>(
                OrganizationSettingsErrors.InvalidAddressCountryCode);
        }

        bool anyAddressFieldProvided =
            normalizedLine1 is not null ||
            normalizedLine2 is not null ||
            normalizedPostalCode is not null ||
            normalizedCity is not null ||
            normalizedRegion is not null;

        if (anyAddressFieldProvided &&
            (normalizedLine1 is null || normalizedPostalCode is null || normalizedCity is null))
        {
            return Result.Failure<OrganizationAddress>(
                OrganizationSettingsErrors.IncompleteAddress);
        }

        if (normalizedLine1?.Length > AddressLineMaximumLength ||
            normalizedLine2?.Length > AddressLineMaximumLength ||
            normalizedPostalCode?.Length > PostalCodeMaximumLength ||
            normalizedCity?.Length > CityMaximumLength ||
            normalizedRegion?.Length > RegionMaximumLength)
        {
            return Result.Failure<OrganizationAddress>(
                OrganizationSettingsErrors.InvalidAddress);
        }

        return Result.Success(
            new OrganizationAddress(
                normalizedLine1,
                normalizedLine2,
                normalizedPostalCode,
                normalizedCity,
                normalizedRegion,
                normalizedCountryCode));
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
