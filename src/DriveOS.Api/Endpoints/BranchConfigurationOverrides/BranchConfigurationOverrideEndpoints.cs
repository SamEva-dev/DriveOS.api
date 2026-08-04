using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Archive;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.CreateDraft;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetById;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetVersions;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Publish;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.UpdateDraft;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.BranchConfigurationOverrides;

public static class BranchConfigurationOverrideEndpoints
{
    public static IEndpointRouteBuilder MapBranchConfigurationOverrideEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/branches/{branchId:guid}/configuration-overrides")
            .WithTags("Branch configuration overrides");

        group.MapGet("/", GetVersionsAsync)
            .WithName("GetBranchConfigurationOverrideVersions")
            .Produces<IReadOnlyList<BranchConfigurationOverrideListItemResponseContract>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Read);

        group.MapGet("/{overrideId:guid}", GetByIdAsync)
            .WithName("GetBranchConfigurationOverride")
            .Produces<BranchConfigurationOverrideResponseContract>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Read);

        group.MapPost("/", CreateDraftAsync)
            .WithName("CreateBranchConfigurationOverrideDraft")
            .Accepts<CreateBranchConfigurationOverrideDraftRequest>("application/json")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Create);

        group.MapPut("/{overrideId:guid}", UpdateDraftAsync)
            .WithName("UpdateBranchConfigurationOverrideDraft")
            .Accepts<UpdateBranchConfigurationOverrideDraftRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Update);

        group.MapPost("/{overrideId:guid}/publish", PublishAsync)
            .WithName("PublishBranchConfigurationOverride")
            .Accepts<PublishBranchConfigurationOverrideRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Publish);

        group.MapPost("/{overrideId:guid}/archive", ArchiveAsync)
            .WithName("ArchiveBranchConfigurationOverride")
            .Accepts<ArchiveBranchConfigurationOverrideRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.BranchConfigurationOverrides.Archive);

        return endpoints;
    }

    private static async Task<IResult> GetVersionsAsync(
        Guid organizationId,
        Guid branchId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetScope(organizationId, branchId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch, out IResult? failure))
            return failure!;

        Result<IReadOnlyList<BranchConfigurationOverrideListItemResponse>> result =
            await mediator.Send(
                new GetBranchConfigurationOverrideVersionsQuery(organization, branch),
                cancellationToken);

        if (result.IsFailure) return result.Error.ToHttpResult(httpContext);

        var response = result.Value
            .Select(item => mapper.Map<BranchConfigurationOverrideListItemResponse,
                BranchConfigurationOverrideListItemResponseContract>(item))
            .ToArray();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid organizationId,
        Guid branchId,
        Guid overrideId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetIds(organizationId, branchId, overrideId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch,
                out BranchConfigurationOverrideId branchOverride, out IResult? failure))
            return failure!;

        Result<BranchConfigurationOverrideResponse> result = await mediator.Send(
            new GetBranchConfigurationOverrideQuery(organization, branch, branchOverride),
            cancellationToken);

        if (result.IsFailure) return result.Error.ToHttpResult(httpContext);

        return Results.Ok(mapper.Map<BranchConfigurationOverrideResponse,
            BranchConfigurationOverrideResponseContract>(result.Value));
    }

    private static async Task<IResult> CreateDraftAsync(
        Guid organizationId,
        Guid branchId,
        CreateBranchConfigurationOverrideDraftRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetScope(organizationId, branchId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch, out IResult? failure))
            return failure!;

        if (request.BaseConfigurationId == Guid.Empty)
            return BranchConfigurationOverrideErrors.EmptyBaseConfigurationId
                .ToHttpResult(httpContext);

        var model = new CreateBranchConfigurationOverrideDraftApiModel(
            organization,
            branch,
            new OrganizationConfigurationId(request.BaseConfigurationId),
            request.VersionNumber,
            request.CountryCode,
            request.PayloadJson);

        Result<BranchConfigurationOverrideId> result = await mediator.Send(
            mapper.Map<CreateBranchConfigurationOverrideDraftApiModel,
                CreateBranchConfigurationOverrideDraftCommand>(model),
            cancellationToken);

        if (result.IsFailure) return result.Error.ToHttpResult(httpContext);

        return Results.Created(
            $"/api/organizations/{organizationId}/branches/{branchId}/configuration-overrides/{result.Value.Value}",
            new { id = result.Value.Value });
    }

    private static async Task<IResult> UpdateDraftAsync(
        Guid organizationId,
        Guid branchId,
        Guid overrideId,
        UpdateBranchConfigurationOverrideDraftRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetIds(organizationId, branchId, overrideId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch,
                out BranchConfigurationOverrideId branchOverride, out IResult? failure))
            return failure!;

        var model = new UpdateBranchConfigurationOverrideDraftApiModel(
            organization, branch, branchOverride, request.PayloadJson, request.ExpectedRevision);

        Result result = await mediator.Send(
            mapper.Map<UpdateBranchConfigurationOverrideDraftApiModel,
                UpdateBranchConfigurationOverrideDraftCommand>(model),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static async Task<IResult> PublishAsync(
        Guid organizationId,
        Guid branchId,
        Guid overrideId,
        PublishBranchConfigurationOverrideRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetIds(organizationId, branchId, overrideId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch,
                out BranchConfigurationOverrideId branchOverride, out IResult? failure))
            return failure!;

        var model = new PublishBranchConfigurationOverrideApiModel(
            organization,
            branch,
            branchOverride,
            request.EffectiveFromUtc,
            request.EffectiveToUtc,
            request.ExpectedRevision);

        Result result = await mediator.Send(
            mapper.Map<PublishBranchConfigurationOverrideApiModel,
                PublishBranchConfigurationOverrideCommand>(model),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ArchiveAsync(
        Guid organizationId,
        Guid branchId,
        Guid overrideId,
        ArchiveBranchConfigurationOverrideRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!TryGetIds(organizationId, branchId, overrideId, currentTenant, httpContext,
                out OrganizationId organization, out BranchId branch,
                out BranchConfigurationOverrideId branchOverride, out IResult? failure))
            return failure!;

        var model = new ArchiveBranchConfigurationOverrideApiModel(
            organization, branch, branchOverride, request.ExpectedRevision);

        Result result = await mediator.Send(
            mapper.Map<ArchiveBranchConfigurationOverrideApiModel,
                ArchiveBranchConfigurationOverrideCommand>(model),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static bool TryGetIds(
        Guid rawOrganizationId,
        Guid rawBranchId,
        Guid rawOverrideId,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        out OrganizationId organizationId,
        out BranchId branchId,
        out BranchConfigurationOverrideId overrideId,
        out IResult? failure)
    {
        overrideId = default;

        if (!TryGetScope(rawOrganizationId, rawBranchId, currentTenant, httpContext,
                out organizationId, out branchId, out failure))
            return false;

        if (rawOverrideId == Guid.Empty)
        {
            failure = BranchConfigurationOverrideErrors.EmptyId.ToHttpResult(httpContext);
            return false;
        }

        overrideId = new BranchConfigurationOverrideId(rawOverrideId);
        return true;
    }

    private static bool TryGetScope(
        Guid rawOrganizationId,
        Guid rawBranchId,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        out OrganizationId organizationId,
        out BranchId branchId,
        out IResult? failure)
    {
        organizationId = default;
        branchId = default;
        failure = null;

        if (rawOrganizationId == Guid.Empty)
        {
            failure = BranchConfigurationOverrideErrors.EmptyOrganizationId
                .ToHttpResult(httpContext);
            return false;
        }

        if (rawBranchId == Guid.Empty)
        {
            failure = BranchConfigurationOverrideErrors.EmptyBranchId
                .ToHttpResult(httpContext);
            return false;
        }

        organizationId = new OrganizationId(rawOrganizationId);
        branchId = new BranchId(rawBranchId);

        if (!currentTenant.HasTenant || currentTenant.OrganizationId != organizationId)
        {
            failure = Error.Forbidden(
                    "BranchConfigurationOverrides.Tenant.Forbidden",
                    "errors.branchConfigurationOverride.tenant.forbidden")
                .ToHttpResult(httpContext);
            return false;
        }

        return true;
    }
}
