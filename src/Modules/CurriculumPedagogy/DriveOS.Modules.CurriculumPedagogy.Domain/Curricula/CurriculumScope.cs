using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;

public sealed record CurriculumScope
{
    private CurriculumScope() { }

    private CurriculumScope(string countryCode, string licenseCategoryCode)
    {
        CountryCode = countryCode;
        LicenseCategoryCode = licenseCategoryCode;
    }

    public string CountryCode { get; private init; } = string.Empty;

    public string LicenseCategoryCode { get; private init; } = string.Empty;

    public static Result<CurriculumScope> Create(string countryCode, string licenseCategoryCode)
    {
        string normalizedCountryCode = (countryCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedCountryCode.Length != 2 || !normalizedCountryCode.All(char.IsAsciiLetter))
            return Result.Failure<CurriculumScope>(CurriculumErrors.InvalidCountryCode);

        string normalizedCategoryCode = (licenseCategoryCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedCategoryCode.Length is < 1 or > 20 || !normalizedCategoryCode.All(IsCodeCharacter))
            return Result.Failure<CurriculumScope>(CurriculumErrors.InvalidLicenseCategoryCode);

        return Result.Success(new CurriculumScope(normalizedCountryCode, normalizedCategoryCode));
    }

    private static bool IsCodeCharacter(char value) =>
        char.IsAsciiLetterOrDigit(value) || value is '-' or '_' or '.';
}
