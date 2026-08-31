using DomainRelay.Abstractions;
using DriveOS.Modules.ProfessionalMarketplace.Application.Search;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalSearchEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/professional-marketplace/search/professionals",Search)
            .WithTags("Professional Marketplace - Search")
            .RequireAuthorization("ProfessionalMarketplace.Search.Read");
        return app;
    }

    private static async Task<IResult> Search(
        [AsParameters] ProfessionalSearchRequest q,
        IMediator mediator,
        CancellationToken ct)
    {
        var result=await mediator.Send(new SearchProfessionalsQuery(
            q.CountryCode,q.TeachingCategoryCode,q.LanguageCode,q.SpecializationCode,q.AreaCode,
            q.Latitude,q.Longitude,q.RadiusKm,q.AvailableOnDate,q.AvailableFrom,q.AvailableTo,
            q.MaximumRateAmount,q.Currency,q.RateUnit,q.VerifiedOnly,q.Page,q.PageSize),ct);
        return result.IsSuccess?Results.Ok(result.Value):Results.BadRequest(new{code=result.Error.Code,messageKey=result.Error.MessageKey});
    }
}

internal sealed record ProfessionalSearchRequest(
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
    bool VerifiedOnly=true,
    int Page=1,
    int PageSize=20);
