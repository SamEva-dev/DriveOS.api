using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Contracts;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.GetOrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.Modules.Organizations.Application.Organizations.Lifecycle;
using DriveOS.Modules.Organizations.Application.Organizations.OrganizationStatusHistory;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints.Organization.Organizations;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder
        MapOrganizationEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group =
            endpoints.MapGroup("/api/organizations")
                .WithTags("Organizations");

        group.MapPost(
                "/",
                CreateOrganizationAsync)
            .WithName("CreateOrganization")
            .WithSummary("Créer une organisation")
            .Accepts<CreateOrganizationRequest>("application/json")
            .Produces<CreateOrganizationResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.Organizations.Create);

        group.MapGet(
                "/{organizationId:guid}",
                GetOrganizationByIdAsync)
            .WithName("GetOrganizationById")
            .WithSummary(
                "Obtenir une organisation")
            .WithDescription(
                "Retourne les informations principales " +
                "d’une organisation DriveOS.")
            .Produces<GetOrganizationResponse>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Read);

        group.MapGet(
                "/",
                GetOrganizationsAsync)
            .WithName("GetOrganizations")
            .WithSummary(
                "Lister les organisations")
            .WithDescription(
                "Retourne une liste paginée, filtrée " +
                "et triée des organisations DriveOS.")
            .Produces<
                PagedResponse<
                    OrganizationListItemResponse>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Read);

        group.MapGet(
                "/{organizationId:guid}/activation-readiness",
                GetOrganizationActivationReadinessAsync)
            .WithName("GetOrganizationActivationReadiness")
            .WithSummary("Évaluer les prérequis d’activation d’une organisation")
            .Produces<OrganizationActivationReadinessResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.Organizations.Read);

        group.MapGet(
                "/{organizationId:guid}/status-history",
                GetOrganizationStatusHistoryAsync)
            .WithName("GetOrganizationStatusHistory")
            .WithSummary("Obtenir l’historique des statuts d’une organisation")
            .Produces<IReadOnlyList<OrganizationStatusHistoryItem>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.StatusHistoryRead);

        group.MapPost(
            "/{organizationId:guid}/submit-for-activation",
            SubmitForActivationAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.SubmitForActivation);

        group.MapPost(
            "/{organizationId:guid}/activate",
            ActivateAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Activate);

        group.MapPost(
            "/{organizationId:guid}/restrict",
            RestrictAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Restrict);

        group.MapPost(
            "/{organizationId:guid}/suspend",
            SuspendAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Suspend);

        group.MapPost(
            "/{organizationId:guid}/reactivate",
            ReactivateAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Reactivate);

        group.MapPost(
            "/{organizationId:guid}/close",
            CloseAsync)
            .RequireAuthorization(
                DriveOsPermissionCodes.Organizations.Close);

        return endpoints;
    }


    private static async Task<IResult> CreateOrganizationAsync(
        CreateOrganizationRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        CreateOrganizationCommand command =
            mapper.Map<CreateOrganizationRequest, CreateOrganizationCommand>(request);

        Result<OrganizationId> result =
            await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(httpContext);
        }

        Guid organizationId = result.Value.Value;

        return Results.Created(
            $"/api/organizations/{organizationId:D}",
            new CreateOrganizationResponse(organizationId));
    }

    private static async Task<IResult>
    GetOrganizationByIdAsync(
        Guid organizationId,
        IMediator mediator,
        IObjectMapper mapper,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
        {
            return OrganizationErrors.InvalidId
                .ToHttpResult(httpContext);
        }
        var query = new GetOrganizationByIdQuery(
            new OrganizationId(organizationId));

        Result<OrganizationResponse> result =
            await mediator.Send(
                query,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(
                httpContext);
        }

        GetOrganizationResponse response =
            mapper.Map<
                    OrganizationResponse,
                    GetOrganizationResponse>(
                    result.Value);

        return Results.Ok(response);
    }

    private static async Task<IResult>
    GetOrganizationsAsync(
        [AsParameters]
        GetOrganizationsRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        OrganizationSortField sortBy =
            ParseSortField(request.SortBy);

        SortDirection sortDirection =
            ParseSortDirection(
                request.SortDirection);

        var query = new GetOrganizationsQuery(
            request.PageNumber,
            request.PageSize,
            request.Search,
            sortBy,
            sortDirection);

        Result<PagedResult<OrganizationListItem>>
            result =
                await mediator.Send(
                    query,
                    cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(
                httpContext);
        }

        PagedResult<OrganizationListItem> page =
            result.Value;

        List<OrganizationListItemResponse> items =
        mapper.Map<
            List<OrganizationListItem>,
            List<OrganizationListItemResponse>>(
            page.Items.ToList());

        var response =
            new PagedResponse<
                OrganizationListItemResponse>(
                items,
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.TotalPages,
                page.HasPreviousPage,
                page.HasNextPage);

        return Results.Ok(response);
    }

    private static OrganizationSortField
    ParseSortField(string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant()
            switch
        {
            "countrycode" =>
                OrganizationSortField.CountryCode,

            "type" =>
                OrganizationSortField.Type,

            "status" =>
                OrganizationSortField.Status,

            "createdatutc" or "createdat" =>
                OrganizationSortField.CreatedAtUtc,

            _ =>
                OrganizationSortField.LegalName
        };
    }

    private static SortDirection
        ParseSortDirection(
            string? sortDirection)
    {
        return string.Equals(
            sortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Descending
                : SortDirection.Ascending;
    }



    private static async Task<IResult> GetOrganizationActivationReadinessAsync(
        Guid organizationId,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
        {
            return OrganizationErrors.InvalidId.ToHttpResult(httpContext);
        }

        var query = new GetOrganizationActivationReadinessQuery(
            new OrganizationId(organizationId));

        Result<OrganizationActivationReadinessReport> result =
            await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(httpContext);
        }

        static OrganizationActivationRequirementResponse MapRequirement(
            OrganizationActivationRequirementResult requirement) =>
            new(
                requirement.Code,
                requirement.IsSatisfied,
                requirement.Severity.ToString(),
                requirement.MessageKey,
                requirement.Parameters);

        OrganizationActivationReadinessReport report = result.Value;

        var response = new OrganizationActivationReadinessResponse(
            report.OrganizationId.Value,
            report.IsReady,
            report.Requirements.Select(MapRequirement).ToArray(),
            report.BlockingRequirements.Select(MapRequirement).ToArray());

        return Results.Ok(response);
    }

    private static async Task<IResult> GetOrganizationStatusHistoryAsync(
        Guid organizationId,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (organizationId == Guid.Empty)
        {
            return OrganizationErrors.InvalidId.ToHttpResult(httpContext);
        }

        var query = new GetOrganizationStatusHistoryQuery(organizationId);

        Result<IReadOnlyList<OrganizationStatusHistoryItem>> result =
            await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ChangeStatusAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    OrganizationStatus targetStatus,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken)
    {
        ChangeOrganizationStatusCommand command =
            new(
                organizationId,
                targetStatus,
                request.Reason);

        Result result =
            await mediator.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(
                httpContext);
        }

        return Results.NoContent();
    }

    private static Task<IResult> SubmitForActivationAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.PendingActivation,
        mediator,
        httpContext,
        cancellationToken);

    private static Task<IResult> ActivateAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.Active,
        mediator,
        httpContext,
        cancellationToken);

    private static Task<IResult> RestrictAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.Restricted,
        mediator,
        httpContext,
        cancellationToken);
    private static Task<IResult> SuspendAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.Suspended,
        mediator,
        httpContext,
        cancellationToken);

    private static Task<IResult> ReactivateAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.Active,
        mediator,
        httpContext,
        cancellationToken);

    private static Task<IResult> CloseAsync(
    Guid organizationId,
    ChangeOrganizationStatusRequest request,
    IMediator mediator,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
    ChangeStatusAsync(
        organizationId,
        request,
        OrganizationStatus.Closed,
        mediator,
        httpContext,
        cancellationToken);

}