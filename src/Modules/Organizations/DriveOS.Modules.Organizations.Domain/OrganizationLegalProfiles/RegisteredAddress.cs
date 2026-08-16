using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

public sealed record RegisteredAddress
{
    private RegisteredAddress(
        string line1,
        string? line2,
        string postalCode,
        string city,
        string? region,
        string countryCode
    )
    {
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
        City = city;
        Region = region;
        CountryCode = countryCode;
    }

    public string Line1 { get; }
    public string? Line2 { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string? Region { get; }
    public string CountryCode { get; }

    public static Result<RegisteredAddress> Create(
        string line1,
        string? line2,
        string postalCode,
        string city,
        string? region,
        string countryCode
    )
    {
        string normalizedLine1 = line1?.Trim() ?? string.Empty;
        string normalizedPostalCode = postalCode?.Trim() ?? string.Empty;
        string normalizedCity = city?.Trim() ?? string.Empty;
        string normalizedCountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (
            string.IsNullOrWhiteSpace(normalizedLine1)
            || string.IsNullOrWhiteSpace(normalizedPostalCode)
            || string.IsNullOrWhiteSpace(normalizedCity)
            || normalizedCountryCode.Length != 2
            || !normalizedCountryCode.All(char.IsLetter)
        )
        {
            return Result.Failure<RegisteredAddress>(
                OrganizationLegalProfileErrors.InvalidRegisteredAddress
            );
        }

        if (
            normalizedLine1.Length > 200
            || (line2?.Trim().Length ?? 0) > 200
            || normalizedPostalCode.Length > 30
            || normalizedCity.Length > 120
            || (region?.Trim().Length ?? 0) > 120
        )
        {
            return Result.Failure<RegisteredAddress>(
                OrganizationLegalProfileErrors.InvalidRegisteredAddress
            );
        }

        return Result.Success(
            new RegisteredAddress(
                normalizedLine1,
                string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
                normalizedPostalCode,
                normalizedCity,
                string.IsNullOrWhiteSpace(region) ? null : region.Trim(),
                normalizedCountryCode
            )
        );
    }
}
