using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Api.Security;
using DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Provisioning;

public static class ProvisioningEndpoints
{
    public static IEndpointRouteBuilder MapProvisioningEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/provisioning/organizations")
            .WithTags("Provisioning")
            .AddEndpointFilter<AuthGateMachineTokenEndpointFilter>();

        group
            .MapPost("/", ProvisionOrganizationAsync)
            .WithName("ProvisionDriveOsOrganization")
            .WithSummary("Créer une organisation DriveOS depuis AuthGate")
            .Produces<ProvisionOrganizationResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group
            .MapGet("/{organizationId:guid}", VerifyOrganizationAsync)
            .WithName("VerifyDriveOsOrganization")
            .WithSummary("Vérifier une organisation DriveOS depuis AuthGate")
            .Produces<VerifyProvisionedOrganizationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> ProvisionOrganizationAsync(
        ProvisionOrganizationRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (request.ExternalUserId == Guid.Empty)
        {
            return Results.BadRequest(
                new
                {
                    code = "provisioning.external_user_id_required",
                    message = "ExternalUserId is required.",
                }
            );
        }

        var command = new CreateOrganizationCommand(
            request.LegalName,
            request.CountryCode,
            request.OrganizationType
        );

        Result<OrganizationId> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(httpContext);
        }

        Guid organizationId = result.Value.Value;

        return Results.Created(
            $"/api/provisioning/organizations/{organizationId:D}",
            new ProvisionOrganizationResponse(organizationId, OrganizationStatus.Draft.ToString())
        );
    }

    private static async Task<IResult> VerifyOrganizationAsync(
        Guid organizationId,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (organizationId == Guid.Empty)
        {
            return Results.NotFound();
        }

        var query = new GetOrganizationByIdQuery(new OrganizationId(organizationId));

        Result<OrganizationResponse> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error == OrganizationErrors.NotFound)
            {
                return Results.NotFound();
            }

            return result.Error.ToHttpResult(httpContext);
        }

        OrganizationResponse organization = result.Value;
        bool canAuthenticate =
            organization.Status
            is nameof(OrganizationStatus.Draft)
                or nameof(OrganizationStatus.PendingActivation)
                or nameof(OrganizationStatus.Active)
                or nameof(OrganizationStatus.Restricted);

        return Results.Ok(
            new VerifyProvisionedOrganizationResponse(
                organization.Id,
                organization.LegalName,
                organization.Status,
                canAuthenticate
            )
        );
    }
}
