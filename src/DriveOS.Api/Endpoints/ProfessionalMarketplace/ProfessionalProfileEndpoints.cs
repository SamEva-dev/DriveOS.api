using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalProfileEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/professional-marketplace/me/profile", GetCurrent)
            .WithTags("Professional Marketplace - Professional Portal")
            .RequireAuthorization("ProfessionalMarketplace.Profile.Read");

        RouteGroupBuilder group = app.MapGroup("/api/professional-marketplace/profiles").WithTags("Professional Marketplace - Profiles");
        group.MapGet("/{profileId:guid}", Get).RequireAuthorization("ProfessionalMarketplace.Profile.Read");
        group.MapPost("/", Create).RequireAuthorization("ProfessionalMarketplace.Profile.Create");
        group.MapPut("/{profileId:guid}/business-identity", UpdateBusinessIdentity).RequireAuthorization("ProfessionalMarketplace.Profile.Update");
        group.MapPut("/{profileId:guid}/presentation", UpdatePresentation).RequireAuthorization("ProfessionalMarketplace.Profile.Update");
        group.MapPut("/{profileId:guid}/teaching-capabilities", UpdateTeachingCapabilities).RequireAuthorization("ProfessionalMarketplace.Profile.Update");
        group.MapPut("/{profileId:guid}/service-areas", UpdateServiceAreas).RequireAuthorization("ProfessionalMarketplace.ServiceAreas.Manage");
        group.MapPut("/{profileId:guid}/availability", UpdateAvailability).RequireAuthorization("ProfessionalMarketplace.Availability.Manage");
        group.MapPut("/{profileId:guid}/rates", UpdateRates).RequireAuthorization("ProfessionalMarketplace.Rates.Manage");
        group.MapPut("/{profileId:guid}/personal-vehicle", UpdateVehicle).RequireAuthorization("ProfessionalMarketplace.Profile.Update");
        group.MapPut("/{profileId:guid}/engagement-preferences", UpdateEngagementPreferences).RequireAuthorization("ProfessionalMarketplace.Profile.Update");
        group.MapPost("/{profileId:guid}/complete", Complete).RequireAuthorization("ProfessionalMarketplace.Profile.Complete");
        return app;
    }

    private static async Task<IResult> GetCurrent(IMediator mediator, ICurrentUser user, CancellationToken ct)
    {
        if (user.UserId is not { } userId) return Results.Unauthorized();
        Result<ProfessionalProfileResponse> r = await mediator.Send(new GetCurrentProfessionalProfileQuery(userId), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }

    private static async Task<IResult> Get(Guid profileId, IMediator mediator, CancellationToken ct)
    {
        Result<ProfessionalProfileResponse> r = await mediator.Send(new GetProfessionalProfileQuery(new ProfessionalProfileId(profileId)), ct);
        return r.IsSuccess ? Results.Ok(r.Value) : ToProblem(r.Error);
    }

    private static async Task<IResult> Create(CreateProfileRequest q, IMediator mediator, ICurrentUser user, CancellationToken ct)
    {
        if (user.UserId is not { } actor) return Results.Unauthorized();
        ProfessionalProfileId id = q.ProfileId is { } raw && raw != Guid.Empty ? new ProfessionalProfileId(raw) : ProfessionalProfileId.New();
        UserId? linked = q.UserId is { } uid && uid != Guid.Empty ? new UserId(uid) : null;
        Result<ProfessionalProfileId> r = await mediator.Send(new CreateProfessionalProfileCommand(id, new PersonId(q.PersonId), new OrganizationId(q.ProviderOrganizationId), linked, actor), ct);
        return r.IsSuccess ? Results.Created($"/api/professional-marketplace/profiles/{r.Value.Value}", new { id = r.Value.Value }) : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateBusinessIdentity(Guid profileId, BusinessIdentityRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        if (!Enum.TryParse<ProfessionalType>(q.ProfessionalType, true, out var type)) return Results.BadRequest(new { code = "ProfessionalMarketplace.Profile.InvalidProfessionalType", messageKey = "errors.professionalMarketplace.profile.invalidProfessionalType" });
        Result r = await m.Send(new UpdateProfessionalBusinessIdentityCommand(new(profileId), type, q.LegalName, q.TradeName, q.LegalStatusCode, q.RegistrationNumber, q.TaxNumber, q.ProfessionalEmail, q.ProfessionalPhone, q.AddressLine1, q.AddressLine2, q.PostalCode, q.City, q.CountryCode, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdatePresentation(Guid profileId, PresentationRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        Result r = await m.Send(new UpdateProfessionalPresentationCommand(new(profileId), q.Headline, q.Biography, q.ExperienceYears, q.Languages ?? [], q.TeachingCategoryCodes ?? [], q.SpecializationCodes, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }


    private static async Task<IResult> UpdateTeachingCapabilities(Guid profileId, TeachingCapabilitiesRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        TeachingCapabilityInput[] capabilities = (q.Capabilities ?? [])
            .Select(x => new TeachingCapabilityInput(
                x.CategoryCode,
                x.DeliveryModeCodes ?? [],
                x.AudienceCodes ?? [],
                x.LanguageCodes ?? [],
                x.SpecializationCodes ?? []))
            .ToArray();
        Result r = await m.Send(new ReplaceTeachingCapabilitiesCommand(new(profileId), capabilities, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateServiceAreas(Guid profileId, ServiceAreasRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        ProfessionalServiceAreaInput[] areas=(q.Areas??[])
            .Select(x=>new ProfessionalServiceAreaInput(x.AreaCode,x.CountryCode,x.DisplayName,x.Latitude,x.Longitude,x.RadiusKm,x.Primary,x.MobilityMode))
            .ToArray();
        Result r=await m.Send(new ReplaceProfessionalServiceAreasCommand(new(profileId),areas,actor),ct);
        return r.IsSuccess?Results.NoContent():ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateAvailability(Guid profileId, AvailabilityRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        var rules=(q.RecurringRules??[]).Select(x=>new MarketplaceAvailabilityRuleInput(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)).ToArray();
        var exceptions=(q.Exceptions??[]).Select(x=>new MarketplaceAvailabilityExceptionInput(x.Date,x.StartTime,x.EndTime,x.Type,x.Reason)).ToArray();
        Result r=await m.Send(new ReplaceMarketplaceAvailabilityCommand(
            new(profileId),rules,exceptions,q.MinimumBookingNoticeHours,q.MaximumDailyWorkMinutes,q.MaximumConsecutiveWorkMinutes,actor),ct);
        return r.IsSuccess?Results.NoContent():ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateRates(Guid profileId, RatesRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        ProfessionalRateInput[] rates=(q.Rates??[])
            .Select(x=>new ProfessionalRateInput(
                x.RateCode,x.Unit,x.Amount,x.Currency,x.TeachingCategoryCode,x.VehicleProvisionMode,
                x.MileageRate,x.MinimumBillableQuantity,x.Negotiable,x.EffectiveFrom,x.EffectiveTo))
            .ToArray();
        Result r=await m.Send(new ReplaceProfessionalRatesCommand(new(profileId),rates,actor),ct);
        return r.IsSuccess?Results.NoContent():ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateVehicle(Guid profileId, VehicleRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        Result r = await m.Send(new UpdateProfessionalVehicleCommand(new(profileId), q.HasPersonalTrainingVehicle, q.Notes, actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> UpdateEngagementPreferences(Guid profileId, EngagementPreferencesRequest q, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        var parsed = new List<ProfessionalEngagementType>();
        foreach (string raw in q.EngagementTypes ?? [])
        {
            if (!Enum.TryParse<ProfessionalEngagementType>(raw, true, out var value)) return Results.BadRequest(new { code = "ProfessionalMarketplace.Profile.InvalidEngagementPreferences", messageKey = "errors.professionalMarketplace.profile.invalidEngagementPreferences" });
            parsed.Add(value);
        }
        Result r = await m.Send(new UpdateProfessionalEngagementPreferencesCommand(new(profileId), parsed.ToArray(), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static async Task<IResult> Complete(Guid profileId, IMediator m, ICurrentUser u, CancellationToken ct)
    {
        if (u.UserId is not { } actor) return Results.Unauthorized();
        Result r = await m.Send(new CompleteProfessionalProfileCommand(new(profileId), actor), ct);
        return r.IsSuccess ? Results.NoContent() : ToProblem(r.Error);
    }

    private static IResult ToProblem(Error e) => e.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { code = e.Code, messageKey = e.Message }),
        ErrorType.Conflict => Results.Conflict(new { code = e.Code, messageKey = e.Message }),
        _ => Results.BadRequest(new { code = e.Code, messageKey = e.Message })
    };
}

internal sealed record CreateProfileRequest(Guid? ProfileId, Guid PersonId, Guid ProviderOrganizationId, Guid? UserId);
internal sealed record BusinessIdentityRequest(string ProfessionalType, string LegalName, string? TradeName, string LegalStatusCode, string RegistrationNumber, string? TaxNumber, string ProfessionalEmail, string? ProfessionalPhone, string AddressLine1, string? AddressLine2, string PostalCode, string City, string CountryCode);
internal sealed record PresentationRequest(string Headline, string? Biography, int ExperienceYears, string[]? Languages, string[]? TeachingCategoryCodes, string[]? SpecializationCodes);
internal sealed record VehicleRequest(bool HasPersonalTrainingVehicle, string? Notes);
internal sealed record EngagementPreferencesRequest(string[]? EngagementTypes);

internal sealed record TeachingCapabilityRequest(string CategoryCode, string[]? DeliveryModeCodes, string[]? AudienceCodes, string[]? LanguageCodes, string[]? SpecializationCodes);
internal sealed record TeachingCapabilitiesRequest(TeachingCapabilityRequest[]? Capabilities);

internal sealed record ServiceAreaItemRequest(string AreaCode,string CountryCode,string DisplayName,decimal? Latitude,decimal? Longitude,int RadiusKm,bool Primary,ProfessionalMobilityMode MobilityMode);
internal sealed record ServiceAreasRequest(ServiceAreaItemRequest[]? Areas);

internal sealed record AvailabilityRuleRequest(DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);
internal sealed record AvailabilityExceptionRequest(DateOnly Date,TimeOnly? StartTime,TimeOnly? EndTime,MarketplaceAvailabilityExceptionType Type,string? Reason);
internal sealed record AvailabilityRequest(
    AvailabilityRuleRequest[]? RecurringRules,
    AvailabilityExceptionRequest[]? Exceptions,
    int MinimumBookingNoticeHours,
    int MaximumDailyWorkMinutes,
    int MaximumConsecutiveWorkMinutes);

internal sealed record ProfessionalRateRequest(
    string RateCode,
    ProfessionalRateUnit Unit,
    decimal Amount,
    string Currency,
    string? TeachingCategoryCode,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? MileageRate,
    decimal? MinimumBillableQuantity,
    bool Negotiable,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);
internal sealed record RatesRequest(ProfessionalRateRequest[]? Rates);
