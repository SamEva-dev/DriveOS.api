using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Search;

public sealed record SearchProfessionalsQuery(
    string? CountryCode,
    string? TeachingCategoryCode,
    string? LanguageCode,
    string? SpecializationCode,
    string? AreaCode,
    decimal? Latitude,
    decimal? Longitude,
    int? RadiusKm,
    DateOnly? AvailableOnDate,
    TimeOnly? AvailableFrom,
    TimeOnly? AvailableTo,
    decimal? MaximumRateAmount,
    string? Currency,
    ProfessionalRateUnit? RateUnit,
    bool VerifiedOnly = true,
    int Page = 1,
    int PageSize = 20) : IQuery<ProfessionalSearchPage>;

public sealed record ProfessionalSearchPage(
    int Page,
    int PageSize,
    int Total,
    ProfessionalSearchResult[] Items);

public sealed record ProfessionalSearchResult(
    Guid ProfileId,
    string Headline,
    string ProfessionalType,
    string VerificationBadge,
    string[] TeachingCategoryCodes,
    string[] Languages,
    string[] SpecializationCodes,
    string? PrimaryArea,
    decimal? DistanceKm,
    bool CommerciallyAvailable,
    decimal? StartingRateAmount,
    string? RateCurrency,
    string? RateUnit,
    bool Negotiable);

public interface IProfessionalSearchReadService
{
    Task<ProfessionalSearchPage> SearchAsync(SearchProfessionalsQuery query,CancellationToken ct=default);
}
