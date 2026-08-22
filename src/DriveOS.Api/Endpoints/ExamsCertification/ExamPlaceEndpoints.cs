using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ExamsCertification.Application.Places;
using DriveOS.Modules.ExamsCertification.Application.Places.Sync;
using DriveOS.Modules.ExamsCertification.Application.Places.Watch;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Application.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ExamsCertification;

internal static class ExamPlaceEndpoints
{
    internal static IEndpointRouteBuilder MapExamPlaceEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams").WithTags("Exams - Centres and places");
        group.MapGet("/centers", GetCenters).RequireAuthorization("ExamPlaces.Read");
        group.MapPost("/centers", CreateCenter).RequireAuthorization("ExamPlaces.Manage");
        group.MapGet("/places", GetPlaces).RequireAuthorization("ExamPlaces.Read");
        group.MapPost("/places", CreatePlace).RequireAuthorization("ExamPlaces.Manage");
        group.MapPost("/places/synchronize", SynchronizePlaces).RequireAuthorization("ExamPlaces.Import");
        group.MapPost("/places/import", ImportPlaces).RequireAuthorization("ExamPlaces.Import");
        group.MapGet("/place-watches", GetPlaceWatches).RequireAuthorization("ExamPlaces.Watch");
        group.MapPost("/place-watches", CreatePlaceWatch).RequireAuthorization("ExamPlaces.Watch");
        group.MapPost("/place-watches/{subscriptionId:guid}/pause", PausePlaceWatch).RequireAuthorization("ExamPlaces.Watch");
        group.MapPost("/place-watches/{subscriptionId:guid}/resume", ResumePlaceWatch).RequireAuthorization("ExamPlaces.Watch");
        group.MapPost("/place-watches/{subscriptionId:guid}/scan", RunPlaceWatch).RequireAuthorization("ExamPlaces.Watch");
        group.MapGet("/place-watches/{subscriptionId:guid}/scans", GetPlaceWatchScans).RequireAuthorization("ExamPlaces.Watch");
        group.MapGet("/providers", GetProviders).RequireAuthorization("ExamProviders.Manage");
        group.MapGet("/provider-connections", GetProviderConnections).RequireAuthorization("ExamProviders.Manage");
        group.MapPost("/provider-connections", CreateProviderConnection).RequireAuthorization("ExamProviders.Manage");
        group.MapPost("/provider-connections/{connectionId:guid}/test", TestProviderConnection).RequireAuthorization("ExamProviders.Manage");
        group.MapPost("/provider-connections/{connectionId:guid}/suspend", SuspendProviderConnection).RequireAuthorization("ExamProviders.Manage");
        group.MapPost("/provider-connections/{connectionId:guid}/revoke", RevokeProviderConnection).RequireAuthorization("ExamProviders.Manage");
        return app;
    }

    private static async Task<IResult> GetCenters(IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamCenterResponse>> result = await mediator.Send(new GetExamCentersQuery(organizationId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateCenter(CreateExamCenterRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamCenterId> result = await mediator.Send(new CreateExamCenterCommand(organizationId, request.Name, request.CountryCode,
            request.TimeZoneId, request.AdministrativeAreaCode, request.Address, request.ExternalProviderCode, request.ExternalCenterId, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Created($"/api/exams/centers/{result.Value.Value}", new { id = result.Value.Value }) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetPlaces(DateTimeOffset fromUtc, DateTimeOffset toUtc, string? licenseCategory, IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamPlaceResponse>> result = await mediator.Send(new GetAvailableExamPlacesQuery(organizationId, fromUtc, toUtc, licenseCategory), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreatePlace(CreateExamPlaceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.TryParse(request.Source, true, out ExamPlaceSource source))
            return Results.BadRequest(new { code = "Exams.Place.InvalidSource", messageKey = "errors.exams.place.invalidSource" });

        Result<ExamPlaceId> result = await mediator.Send(new CreateExamPlaceCommand(organizationId, new ExamCenterId(request.ExamCenterId),
            request.ExamType, request.LicenseCategory, request.StartsAtUtc, request.EndsAtUtc, request.TimeZoneId, source,
            request.ProviderCode, request.ExternalPlaceId, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Created($"/api/exams/places/{result.Value.Value}", new { id = result.Value.Value }) : ToProblem(result.Error);
    }

    private static async Task<IResult> SynchronizePlaces(SynchronizeExamPlacesRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamPlaceSynchronizationResponse> result = await mediator.Send(new SynchronizeExamPlacesCommand(
            organizationId, request.ProviderCode, request.CountryCode, request.AdministrativeAreaCode, request.ExamCategory,
            request.FromUtc, request.ToUtc, request.CenterExternalIds, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> ImportPlaces(ImportExamPlacesRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamPlaceSynchronizationResponse> result = await mediator.Send(new ImportExamPlacesCommand(
            organizationId, request.ProviderCode, request.Rows, actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }


    private static async Task<IResult> GetPlaceWatches(IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamPlaceWatchResponse>> result = await mediator.Send(new GetExamPlaceWatchesQuery(organizationId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreatePlaceWatch(CreateExamPlaceWatchRequest request, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamPlaceWatchSubscriptionId> result = await mediator.Send(new CreateExamPlaceWatchCommand(
            organizationId, request.ProviderCode, request.CountryCode, request.AdministrativeAreaCode, request.ExamCategory,
            request.WindowFromUtc, request.WindowToUtc, request.CheckIntervalMinutes, request.CenterExternalIds, actorUserId), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/exams/place-watches/{result.Value.Value}", new { id = result.Value.Value })
            : ToProblem(result.Error);
    }

    private static async Task<IResult> PausePlaceWatch(Guid subscriptionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result result = await mediator.Send(new PauseExamPlaceWatchCommand(organizationId, new ExamPlaceWatchSubscriptionId(subscriptionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> ResumePlaceWatch(Guid subscriptionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result result = await mediator.Send(new ResumeExamPlaceWatchCommand(organizationId, new ExamPlaceWatchSubscriptionId(subscriptionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> RunPlaceWatch(Guid subscriptionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamPlaceWatchRunResponse> result = await mediator.Send(new RunExamPlaceWatchCommand(
            organizationId, new ExamPlaceWatchSubscriptionId(subscriptionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }


    private static async Task<IResult> GetPlaceWatchScans(Guid subscriptionId, int? take, IMediator mediator, ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamPlaceWatchScanResponse>> result = await mediator.Send(new GetExamPlaceWatchScansQuery(
            organizationId, new ExamPlaceWatchSubscriptionId(subscriptionId), take ?? 50), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }


    private static async Task<IResult> GetProviders(IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamProviderCatalogResponse>> result = await mediator.Send(new GetExamProviderCatalogQuery(organizationId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> GetProviderConnections(IMediator mediator, ICurrentTenant tenant, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyList<ExamProviderConnectionResponse>> result = await mediator.Send(new GetExamProviderConnectionsQuery(organizationId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> CreateProviderConnection(CreateExamProviderConnectionRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.TryParse(request.Kind, true, out ExamPlaceProviderKind kind)
            || !Enum.TryParse(request.AuthenticationMode, true, out ExamProviderAuthenticationMode authMode))
            return Results.BadRequest(new { code = "Exams.ProviderConnection.InvalidProvider", messageKey = "errors.exams.providerConnection.invalidProvider" });

        Result<ExamProviderConnectionId> result = await mediator.Send(new CreateExamProviderConnectionCommand(
            organizationId, request.ProviderCode, request.DisplayName, request.CountryCode, kind, authMode,
            request.BaseUrl, request.CredentialReference, request.RequestsPerMinute, actorUserId), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/exams/provider-connections/{result.Value.Value}", new { id = result.Value.Value })
            : ToProblem(result.Error);
    }

    private static async Task<IResult> TestProviderConnection(Guid connectionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<ExamProviderConnectionTestResponse> result = await mediator.Send(new TestExamProviderConnectionCommand(
            organizationId, new ExamProviderConnectionId(connectionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
    }

    private static async Task<IResult> SuspendProviderConnection(Guid connectionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result result = await mediator.Send(new SuspendExamProviderConnectionCommand(organizationId, new ExamProviderConnectionId(connectionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static async Task<IResult> RevokeProviderConnection(Guid connectionId, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result result = await mediator.Send(new RevokeExamProviderConnectionCommand(organizationId, new ExamProviderConnectionId(connectionId), actorUserId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
    }

    private static IResult ToProblem(Error error) => Results.Problem(statusCode: error.Type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    }, title: error.Code, detail: error.MessageKey);
}

public sealed record CreateExamCenterRequest(string Name, string CountryCode, string TimeZoneId, string? AdministrativeAreaCode,
    string? Address, string? ExternalProviderCode, string? ExternalCenterId);

public sealed record CreateExamPlaceRequest(Guid ExamCenterId, string ExamType, string LicenseCategory, DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc, string TimeZoneId, string Source, string ProviderCode, string? ExternalPlaceId);

public sealed record SynchronizeExamPlacesRequest(string ProviderCode, string CountryCode, string? AdministrativeAreaCode,
    string? ExamCategory, DateTimeOffset FromUtc, DateTimeOffset ToUtc, IReadOnlyCollection<string>? CenterExternalIds);

public sealed record ImportExamPlacesRequest(string ProviderCode, IReadOnlyCollection<ExamPlaceImportRow> Rows);

public sealed record CreateExamPlaceWatchRequest(string ProviderCode, string CountryCode, string? AdministrativeAreaCode,
    string? ExamCategory, DateTimeOffset WindowFromUtc, DateTimeOffset WindowToUtc, int CheckIntervalMinutes,
    IReadOnlyCollection<string>? CenterExternalIds);

public sealed record CreateExamProviderConnectionRequest(string ProviderCode, string DisplayName, string CountryCode,
    string Kind, string AuthenticationMode, string? BaseUrl, string? CredentialReference, int RequestsPerMinute = 60);
