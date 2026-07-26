using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Modules.Organizations.Application
    .Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organizations;

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
        .WithSummary(
            "Créer une organisation")
        .WithDescription(
            "Crée le tenant principal d’une auto-école, " +
            "d’un réseau, d’un centre de formation " +
            "ou d’une autre structure partenaire.")
        .WithTags("Organizations")
        .Accepts<CreateOrganizationRequest>(
            "application/json")
        .Produces<CreateOrganizationResponse>(
            StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(
            StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(
        StatusCodes.Status409Conflict);

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
        StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult>
        CreateOrganizationAsync(
            CreateOrganizationRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand(
            request.LegalName,
            request.CountryCode,
            request.OrganizationType);

        Result<OrganizationId> result =
            await mediator.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(
                httpContext);
        }

        Guid organizationId =
            result.Value.Value;

        var response =
            new CreateOrganizationResponse(
                organizationId);

        return Results.Created(
            $"/api/organizations/{organizationId}",
            response);
    }

    private static async Task<IResult>
    GetOrganizationByIdAsync(
        Guid organizationId,
        IMediator mediator,
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

        OrganizationResponse organization =
            result.Value;

        var response = new GetOrganizationResponse(
            organization.Id,
            organization.LegalName,
            organization.CountryCode,
            organization.Type,
            organization.Status,
            organization.CreatedAtUtc,
            organization.CreatedByUserId,
            organization.LastModifiedAtUtc,
            organization.LastModifiedByUserId);

        return Results.Ok(response);
    }
}