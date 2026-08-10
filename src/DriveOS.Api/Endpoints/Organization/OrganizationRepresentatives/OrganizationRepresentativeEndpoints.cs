using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Create;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.End;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetById;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetList;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Reactivate;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.SetPrimaryOwner;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Suspend;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.UpdateAuthority;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Organization.OrganizationRepresentatives;

public static class OrganizationRepresentativeEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationRepresentativeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/representatives")
            .WithTags("Organization representatives");

        group.MapGet("/", GetListAsync)
            .WithName("GetOrganizationRepresentatives")
            .Produces<IReadOnlyCollection<OrganizationRepresentativeListItemContract>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Read);

        group.MapGet("/{representativeId:guid}", GetByIdAsync)
            .WithName("GetOrganizationRepresentative")
            .Produces<OrganizationRepresentativeResponseContract>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Read);

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrganizationRepresentative")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Create);

        group.MapPut("/{representativeId:guid}/authority", UpdateAuthorityAsync)
            .WithName("UpdateOrganizationRepresentativeAuthority")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Update);

        group.MapPost("/{representativeId:guid}/activate", ActivateAsync)
            .WithName("ActivateOrganizationRepresentative")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Activate);

        group.MapPost("/{representativeId:guid}/suspend", SuspendAsync)
            .WithName("SuspendOrganizationRepresentative")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Suspend);

        group.MapPost("/{representativeId:guid}/reactivate", ReactivateAsync)
            .WithName("ReactivateOrganizationRepresentative")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.Reactivate);

        group.MapPost("/{representativeId:guid}/end", EndAsync)
            .WithName("EndOrganizationRepresentative")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.End);

        group.MapPost("/{representativeId:guid}/set-primary-owner", SetPrimaryOwnerAsync)
            .WithName("SetPrimaryOrganizationOwner")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationRepresentatives.SetPrimaryOwner);

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(
        Guid organizationId,
        OrganizationRepresentativeStatus? status,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryOrganization(organizationId, tenant, context, out OrganizationId organization, out IResult? failure))
            return failure!;

        Result<IReadOnlyCollection<OrganizationRepresentativeListItem>> result =
            await mediator.Send(new GetOrganizationRepresentativesQuery(organization, status), cancellationToken);

        if (result.IsFailure)
            return result.Error.ToHttpResult(context);

        return Results.Ok(result.Value
            .Select(item => mapper.Map<OrganizationRepresentativeListItem, OrganizationRepresentativeListItemContract>(item))
            .ToArray());
    }

    private static async Task<IResult> GetByIdAsync(
        Guid organizationId,
        Guid representativeId,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryIds(organizationId, representativeId, tenant, context, out OrganizationId organization,
                out OrganizationRepresentativeId representative, out IResult? failure))
            return failure!;

        Result<OrganizationRepresentativeResponse> result = await mediator.Send(
            new GetOrganizationRepresentativeByIdQuery(organization, representative), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(mapper.Map<OrganizationRepresentativeResponse, OrganizationRepresentativeResponseContract>(result.Value))
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CreateAsync(
        Guid organizationId,
        CreateOrganizationRepresentativeRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryOrganization(organizationId, tenant, context, out OrganizationId organization, out IResult? failure))
            return failure!;

        if (request.PersonId == Guid.Empty)
            return InvalidIdentifier("personId", context);
        if (request.UserId == Guid.Empty)
            return InvalidIdentifier("userId", context);
        if (!request.PersonId.HasValue && !request.UserId.HasValue)
            return InvalidIdentifier("userId", context);

        // PersonId is an internal domain identifier. The UI must never ask a business user
        // to type a GUID. Until DriveOS has a dedicated Person directory, a representative
        // created from an AuthGate account gets a server-generated PersonId.
        PersonId personId = request.PersonId.HasValue
            ? new PersonId(request.PersonId.Value)
            : PersonId.New();

        var model = new CreateOrganizationRepresentativeApiModel(
            organization,
            personId,
            request.UserId.HasValue ? new UserId(request.UserId.Value) : null,
            request.RepresentativeType,
            request.AuthorityScope,
            request.IsPrimaryOwner,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.ActivateImmediately);

        Result<OrganizationRepresentativeId> result = await mediator.Send(
            mapper.Map<CreateOrganizationRepresentativeApiModel, CreateOrganizationRepresentativeCommand>(model),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created(
                $"/api/organizations/{organizationId}/representatives/{result.Value.Value}",
                new { id = result.Value.Value })
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> UpdateAuthorityAsync(
        Guid organizationId,
        Guid representativeId,
        UpdateOrganizationRepresentativeAuthorityRequest request,
        IMediator mediator,
        IObjectMapper mapper,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (!TryIds(organizationId, representativeId, tenant, context, out OrganizationId organization,
                out OrganizationRepresentativeId representative, out IResult? failure))
            return failure!;
        if (request.UserId == Guid.Empty)
            return InvalidIdentifier("userId", context);

        var model = new UpdateOrganizationRepresentativeAuthorityApiModel(
            organization,
            representative,
            request.AuthorityScope,
            request.UserId.HasValue ? new UserId(request.UserId.Value) : null,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.ExpectedRevision);

        Result result = await mediator.Send(
            mapper.Map<UpdateOrganizationRepresentativeAuthorityApiModel, UpdateOrganizationRepresentativeAuthorityCommand>(model),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static Task<IResult> ActivateAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken) =>
        ChangeStatusAsync(organizationId, representativeId, request, mediator, mapper, tenant, context,
            cancellationToken, isSetPrimary: false);

    private static Task<IResult> SetPrimaryOwnerAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken) =>
        ChangeStatusAsync(organizationId, representativeId, request, mediator, mapper, tenant, context,
            cancellationToken, isSetPrimary: true);

    private static async Task<IResult> ChangeStatusAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken, bool isSetPrimary)
    {
        if (!TryIds(organizationId, representativeId, tenant, context, out OrganizationId organization,
                out OrganizationRepresentativeId representative, out IResult? failure))
            return failure!;

        var model = new ChangeOrganizationRepresentativeStatusApiModel(
            organization, representative, request.ExpectedRevision);

        Result result = isSetPrimary
            ? await mediator.Send(mapper.Map<ChangeOrganizationRepresentativeStatusApiModel, SetPrimaryOrganizationOwnerCommand>(model), cancellationToken)
            : await mediator.Send(mapper.Map<ChangeOrganizationRepresentativeStatusApiModel, ActivateOrganizationRepresentativeCommand>(model), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static Task<IResult> SuspendAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusWithReasonRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken) =>
        ChangeStatusWithReasonAsync(organizationId, representativeId, request, mediator, mapper, tenant, context,
            cancellationToken, reactivate: false);

    private static Task<IResult> ReactivateAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusWithReasonRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken) =>
        ChangeStatusWithReasonAsync(organizationId, representativeId, request, mediator, mapper, tenant, context,
            cancellationToken, reactivate: true);

    private static async Task<IResult> ChangeStatusWithReasonAsync(Guid organizationId, Guid representativeId,
        ChangeOrganizationRepresentativeStatusWithReasonRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken, bool reactivate)
    {
        if (!TryIds(organizationId, representativeId, tenant, context, out OrganizationId organization,
                out OrganizationRepresentativeId representative, out IResult? failure))
            return failure!;

        var model = new ChangeOrganizationRepresentativeStatusWithReasonApiModel(
            organization, representative, request.Reason, request.ExpectedRevision);

        Result result = reactivate
            ? await mediator.Send(mapper.Map<ChangeOrganizationRepresentativeStatusWithReasonApiModel, ReactivateOrganizationRepresentativeCommand>(model), cancellationToken)
            : await mediator.Send(mapper.Map<ChangeOrganizationRepresentativeStatusWithReasonApiModel, SuspendOrganizationRepresentativeCommand>(model), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> EndAsync(Guid organizationId, Guid representativeId,
        EndOrganizationRepresentativeRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken cancellationToken)
    {
        if (!TryIds(organizationId, representativeId, tenant, context, out OrganizationId organization,
                out OrganizationRepresentativeId representative, out IResult? failure))
            return failure!;

        var model = new EndOrganizationRepresentativeApiModel(
            organization, representative, request.EffectiveTo, request.Reason, request.ExpectedRevision);

        Result result = await mediator.Send(
            mapper.Map<EndOrganizationRepresentativeApiModel, EndOrganizationRepresentativeCommand>(model),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static bool TryOrganization(Guid rawOrganizationId, ICurrentTenant tenant, HttpContext context,
        out OrganizationId organizationId, out IResult? failure)
    {
        organizationId = new OrganizationId(rawOrganizationId);
        failure = null;

        if (rawOrganizationId == Guid.Empty)
        {
            failure = InvalidIdentifier("organizationId", context);
            return false;
        }

        if (!tenant.HasTenant || tenant.OrganizationId is null || tenant.OrganizationId.Value != organizationId)
        {
            failure = Results.Forbid();
            return false;
        }

        return true;
    }

    private static bool TryIds(Guid rawOrganizationId, Guid rawRepresentativeId, ICurrentTenant tenant,
        HttpContext context, out OrganizationId organizationId,
        out OrganizationRepresentativeId representativeId, out IResult? failure)
    {
        representativeId = new OrganizationRepresentativeId(rawRepresentativeId);
        if (!TryOrganization(rawOrganizationId, tenant, context, out organizationId, out failure))
            return false;

        if (rawRepresentativeId == Guid.Empty)
        {
            failure = InvalidIdentifier("representativeId", context);
            return false;
        }

        return true;
    }

    private static IResult InvalidIdentifier(string parameterName, HttpContext context)
    {
        var error = Error.Validation(
            "OrganizationRepresentatives.Identifier.Invalid",
            "errors.organizationRepresentative.identifier.invalid",
            new Dictionary<string, object?> { ["parameter"] = parameterName });

        return error.ToHttpResult(context);
    }
}
