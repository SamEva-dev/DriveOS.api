using System.Globalization;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed record OrganizationRegionalSettings
{
    public const int TimeZoneMaximumLength = 100;
    public const int FormatMaximumLength = 50;

    private OrganizationRegionalSettings(
        string defaultLanguage,
        string supportedLanguages,
        string timeZoneId,
        string currencyCode,
        string dateFormat,
        string timeFormat,
        DayOfWeek firstDayOfWeek,
        MeasurementSystem measurementSystem)
    {
        DefaultLanguage = defaultLanguage;
        SupportedLanguages = supportedLanguages;
        TimeZoneId = timeZoneId;
        CurrencyCode = currencyCode;
        DateFormat = dateFormat;
        TimeFormat = timeFormat;
        FirstDayOfWeek = firstDayOfWeek;
        MeasurementSystem = measurementSystem;
    }

    public string DefaultLanguage { get; }
    public string SupportedLanguages { get; }
    public string TimeZoneId { get; }
    public string CurrencyCode { get; }
    public string DateFormat { get; }
    public string TimeFormat { get; }
    public DayOfWeek FirstDayOfWeek { get; }
    public MeasurementSystem MeasurementSystem { get; }

    public IReadOnlyCollection<string> SupportedLanguageCodes =>
        SupportedLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static Result<OrganizationRegionalSettings> Create(
        string? defaultLanguage,
        IEnumerable<string>? supportedLanguages,
        string? timeZoneId,
        string? currencyCode,
        string? dateFormat,
        string? timeFormat,
        DayOfWeek firstDayOfWeek,
        MeasurementSystem measurementSystem)
    {
        string normalizedDefaultLanguage = NormalizeLanguage(defaultLanguage);
        string[] normalizedSupportedLanguages = (supportedLanguages ?? [])
            .Select(NormalizeLanguage)
            .Where(language => language.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(language => language, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!IsValidLanguageCode(normalizedDefaultLanguage) ||
            normalizedSupportedLanguages.Length == 0 ||
            normalizedSupportedLanguages.Any(language => !IsValidLanguageCode(language)))
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.InvalidLanguages);
        }

        if (!normalizedSupportedLanguages.Contains(
                normalizedDefaultLanguage,
                StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.DefaultLanguageNotSupported);
        }

        string normalizedTimeZoneId = timeZoneId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedTimeZoneId) ||
            normalizedTimeZoneId.Length > TimeZoneMaximumLength)
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.InvalidTimeZone);
        }

        string normalizedCurrencyCode = currencyCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrencyCode.Length != 3 ||
            !normalizedCurrencyCode.All(char.IsLetter))
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.InvalidCurrency);
        }

        string normalizedDateFormat = dateFormat?.Trim() ?? string.Empty;
        string normalizedTimeFormat = timeFormat?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedDateFormat) ||
            normalizedDateFormat.Length > FormatMaximumLength ||
            string.IsNullOrWhiteSpace(normalizedTimeFormat) ||
            normalizedTimeFormat.Length > FormatMaximumLength)
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.InvalidDateTimeFormat);
        }

        if (!Enum.IsDefined(firstDayOfWeek) || !Enum.IsDefined(measurementSystem))
        {
            return Result.Failure<OrganizationRegionalSettings>(
                OrganizationSettingsErrors.InvalidRegionalConvention);
        }

        return Result.Success(
            new OrganizationRegionalSettings(
                normalizedDefaultLanguage,
                string.Join(',', normalizedSupportedLanguages),
                normalizedTimeZoneId,
                normalizedCurrencyCode,
                normalizedDateFormat,
                normalizedTimeFormat,
                firstDayOfWeek,
                measurementSystem));
    }

    private static string NormalizeLanguage(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        try
        {
            return CultureInfo.GetCultureInfo(normalized).Name;
        }
        catch (CultureNotFoundException)
        {
            return normalized;
        }
    }

    private static bool IsValidLanguageCode(string languageCode)
    {
        if (languageCode.Length is < 2 or > 15)
        {
            return false;
        }

        return languageCode.All(character =>
            char.IsLetter(character) || character == '-');
    }
}
