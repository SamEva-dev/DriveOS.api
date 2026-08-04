using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed record OrganizationProfile
{
    public const int TradeNameMaximumLength = 200;
    public const int RegistrationNumberMaximumLength = 100;
    public const int TaxNumberMaximumLength = 100;

    private OrganizationProfile(
        string? tradeName,
        string? registrationNumber,
        string? taxNumber)
    {
        TradeName = tradeName;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
    }

    public string? TradeName { get; }
    public string? RegistrationNumber { get; }
    public string? TaxNumber { get; }

    public static Result<OrganizationProfile> Create(
        string? tradeName,
        string? registrationNumber,
        string? taxNumber)
    {
        string? normalizedTradeName = NormalizeOptional(tradeName);
        string? normalizedRegistrationNumber = NormalizeOptional(registrationNumber);
        string? normalizedTaxNumber = NormalizeOptional(taxNumber);

        if (normalizedTradeName?.Length > TradeNameMaximumLength)
        {
            return Result.Failure<OrganizationProfile>(
                OrganizationSettingsErrors.InvalidTradeName);
        }

        if (normalizedRegistrationNumber?.Length > RegistrationNumberMaximumLength)
        {
            return Result.Failure<OrganizationProfile>(
                OrganizationSettingsErrors.InvalidRegistrationNumber);
        }

        if (normalizedTaxNumber?.Length > TaxNumberMaximumLength)
        {
            return Result.Failure<OrganizationProfile>(
                OrganizationSettingsErrors.InvalidTaxNumber);
        }

        return Result.Success(
            new OrganizationProfile(
                normalizedTradeName,
                normalizedRegistrationNumber,
                normalizedTaxNumber));
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
