using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Branches;

public sealed record BranchAddress
{
    public const int AddressLineMaximumLength = 200;
    public const int PostalCodeMaximumLength = 20;
    public const int CityMaximumLength = 120;

    private BranchAddress(
        string line1,
        string? line2,
        string postalCode,
        string city,
        string countryCode
    )
    {
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
        City = city;
        CountryCode = countryCode;
    }

    public string Line1 { get; }
    public string? Line2 { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string CountryCode { get; }

    public static Result<BranchAddress> Create(
        string? line1,
        string? line2,
        string? postalCode,
        string? city,
        string? countryCode
    )
    {
        string normalizedLine1 = line1?.Trim() ?? string.Empty;
        string? normalizedLine2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        string normalizedPostalCode = postalCode?.Trim() ?? string.Empty;
        string normalizedCity = city?.Trim() ?? string.Empty;
        string normalizedCountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (
            string.IsNullOrWhiteSpace(normalizedLine1)
            || normalizedLine1.Length > AddressLineMaximumLength
            || normalizedLine2?.Length > AddressLineMaximumLength
            || string.IsNullOrWhiteSpace(normalizedPostalCode)
            || normalizedPostalCode.Length > PostalCodeMaximumLength
            || string.IsNullOrWhiteSpace(normalizedCity)
            || normalizedCity.Length > CityMaximumLength
        )
        {
            return Result.Failure<BranchAddress>(BranchErrors.InvalidAddress);
        }

        if (normalizedCountryCode.Length != 2 || !normalizedCountryCode.All(char.IsLetter))
        {
            return Result.Failure<BranchAddress>(BranchErrors.InvalidCountryCode);
        }

        return Result.Success(
            new BranchAddress(
                normalizedLine1,
                normalizedLine2,
                normalizedPostalCode,
                normalizedCity,
                normalizedCountryCode
            )
        );
    }
}
