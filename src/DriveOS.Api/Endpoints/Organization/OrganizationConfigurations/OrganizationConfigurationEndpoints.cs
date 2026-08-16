using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.CreateDraft;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetById;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetVersions;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Publish;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.UpdateDraft;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.OrganizationConfigurations;

public static class OrganizationConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationConfigurationEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/configurations")
            .WithTags("Organization configurations");

        group
            .MapGet("/", GetVersionsAsync)
            .WithName("GetOrganizationConfigurationVersions")
            .Produces<IReadOnlyList<OrganizationConfigurationListItemResponseContract>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Read);

        group
            .MapGet("/{configurationId:guid}", GetByIdAsync)
            .WithName("GetOrganizationConfiguration")
            .Produces<OrganizationConfigurationResponseContract>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Read);

        group
            .MapPost("/", CreateDraftAsync)
            .WithName("CreateOrganizationConfigurationDraft")
            .Accepts<CreateOrganizationConfigurationDraftRequest>("application/json")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Create);

        group
            .MapPut("/{configurationId:guid}", UpdateDraftAsync)
            .WithName("UpdateOrganizationConfigurationDraft")
            .Accepts<UpdateOrganizationConfigurationDraftRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Update);

        group
            .MapPost("/{configurationId:guid}/publish", PublishAsync)
            .WithName("PublishOrganizationConfiguration")
            .Accepts<PublishOrganizationConfigurationRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Publish);

        group
            .MapPost("/{configurationId:guid}/archive", ArchiveAsync)
            .WithName("ArchiveOrganizationConfiguration")
            .Accepts<ArchiveOrganizationConfigurationRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Archive);

        return endpoints;
    }

    private static async Task<IResult> GetVersionsAsync(
        Guid organizationId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        Result<IReadOnlyList<OrganizationConfigurationListItemResponse>> result =
            await mediator.Send(
                new GetOrganizationConfigurationVersionsQuery(id),
                cancellationToken
            );

        if (result.IsFailure)
            return result.Error.ToHttpResult(httpContext);

        var response = result
            .Value.Select(item =>
                mapper.Map<
                    OrganizationConfigurationListItemResponse,
                    OrganizationConfigurationListItemResponseContract
                >(item)
            )
            .ToArray();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid organizationId,
        Guid configurationId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        if (
            !TryGetConfigurationId(
                configurationId,
                httpContext,
                out OrganizationConfigurationId configId,
                out failure
            )
        )
            return failure!;

        Result<OrganizationConfigurationResponse> result = await mediator.Send(
            new GetOrganizationConfigurationQuery(id, configId),
            cancellationToken
        );

        if (result.IsFailure)
            return result.Error.ToHttpResult(httpContext);

        return Results.Ok(
            mapper.Map<
                OrganizationConfigurationResponse,
                OrganizationConfigurationResponseContract
            >(result.Value)
        );
    }

    private static async Task<IResult> CreateDraftAsync(
        Guid organizationId,
        CreateOrganizationConfigurationDraftRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetScopedOrganizationId(
                organizationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out IResult? failure
            )
        )
            return failure!;

        var model = new CreateOrganizationConfigurationDraftApiModel(
            id,
            request.VersionNumber,
            request.CountryCode,
            request.PayloadJson
        );

        CreateOrganizationConfigurationDraftCommand command = mapper.Map<
            CreateOrganizationConfigurationDraftApiModel,
            CreateOrganizationConfigurationDraftCommand
        >(model);

        Result<OrganizationConfigurationId> result = await mediator.Send(
            command,
            cancellationToken
        );

        if (result.IsFailure)
            return result.Error.ToHttpResult(httpContext);

        return Results.Created(
            $"/api/organizations/{organizationId}/configurations/{result.Value.Value}",
            new { id = result.Value.Value }
        );
    }

    private static async Task<IResult> UpdateDraftAsync(
        Guid organizationId,
        Guid configurationId,
        UpdateOrganizationConfigurationDraftRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetIds(
                organizationId,
                configurationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out OrganizationConfigurationId configId,
                out IResult? failure
            )
        )
            return failure!;

        var model = new UpdateOrganizationConfigurationDraftApiModel(
            id,
            configId,
            request.PayloadJson,
            request.ExpectedRevision
        );

        Result result = await mediator.Send(
            mapper.Map<
                UpdateOrganizationConfigurationDraftApiModel,
                UpdateOrganizationConfigurationDraftCommand
            >(model),
            cancellationToken
        );

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static async Task<IResult> PublishAsync(
        Guid organizationId,
        Guid configurationId,
        PublishOrganizationConfigurationRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetIds(
                organizationId,
                configurationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out OrganizationConfigurationId configId,
                out IResult? failure
            )
        )
            return failure!;

        var model = new PublishOrganizationConfigurationApiModel(
            id,
            configId,
            request.EffectiveFromUtc,
            request.EffectiveToUtc,
            request.ExpectedRevision
        );

        Result result = await mediator.Send(
            mapper.Map<
                PublishOrganizationConfigurationApiModel,
                PublishOrganizationConfigurationCommand
            >(model),
            cancellationToken
        );

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ArchiveAsync(
        Guid organizationId,
        Guid configurationId,
        ArchiveOrganizationConfigurationRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (
            !TryGetIds(
                organizationId,
                configurationId,
                currentTenant,
                httpContext,
                out OrganizationId id,
                out OrganizationConfigurationId configId,
                out IResult? failure
            )
        )
            return failure!;

        var model = new ArchiveOrganizationConfigurationApiModel(
            id,
            configId,
            request.ExpectedRevision
        );

        Result result = await mediator.Send(
            mapper.Map<
                ArchiveOrganizationConfigurationApiModel,
                ArchiveOrganizationConfigurationCommand
            >(model),
            cancellationToken
        );

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(httpContext);
    }

    private static bool TryGetIds(
        Guid rawOrganizationId,
        Guid rawConfigurationId,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        out OrganizationId organizationId,
        out OrganizationConfigurationId configurationId,
        out IResult? failure
    )
    {
        configurationId = default;

        if (
            !TryGetScopedOrganizationId(
                rawOrganizationId,
                currentTenant,
                httpContext,
                out organizationId,
                out failure
            )
        )
            return false;

        return TryGetConfigurationId(
            rawConfigurationId,
            httpContext,
            out configurationId,
            out failure
        );
    }

    private static bool TryGetScopedOrganizationId(
        Guid rawOrganizationId,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        out OrganizationId organizationId,
        out IResult? failure
    )
    {
        organizationId = default;
        failure = null;

        if (rawOrganizationId == Guid.Empty)
        {
            failure = OrganizationConfigurationErrors.EmptyOrganizationId.ToHttpResult(httpContext);
            return false;
        }

        organizationId = new OrganizationId(rawOrganizationId);

        if (!currentTenant.HasTenant || currentTenant.OrganizationId != organizationId)
        {
            failure = Error
                .Forbidden(
                    "OrganizationConfigurations.Tenant.Forbidden",
                    "errors.organizationConfiguration.tenant.forbidden"
                )
                .ToHttpResult(httpContext);
            return false;
        }

        return true;
    }

    private static bool TryGetConfigurationId(
        Guid rawConfigurationId,
        HttpContext httpContext,
        out OrganizationConfigurationId configurationId,
        out IResult? failure
    )
    {
        configurationId = default;
        failure = null;

        if (rawConfigurationId == Guid.Empty)
        {
            failure = OrganizationConfigurationErrors.EmptyId.ToHttpResult(httpContext);
            return false;
        }

        configurationId = new OrganizationConfigurationId(rawConfigurationId);
        return true;
    }
}
