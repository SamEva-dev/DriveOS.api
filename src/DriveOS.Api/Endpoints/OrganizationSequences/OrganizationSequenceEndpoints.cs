using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Create;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.GetById;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.GetList;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Reactivate;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Reserve;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Suspend;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.OrganizationSequences;

public static class OrganizationSequenceEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationSequenceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/sequences")
            .WithTags("Organization sequences");

        group.MapGet("/", GetListAsync)
            .WithName("GetOrganizationSequences")
            .Produces<IReadOnlyList<OrganizationSequenceListItemContract>>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Read);

        group.MapGet("/{sequenceId:guid}", GetByIdAsync)
            .WithName("GetOrganizationSequence")
            .Produces<OrganizationSequenceResponseContract>()
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Read);

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrganizationSequence")
            .Produces(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Create);

        group.MapPost("/reserve", ReserveAsync)
            .WithName("ReserveOrganizationSequenceNumber")
            .Produces<OrganizationSequenceNumberResponse>()
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Update);

        group.MapPost("/{sequenceId:guid}/suspend", SuspendAsync)
            .WithName("SuspendOrganizationSequence")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Update);

        group.MapPost("/{sequenceId:guid}/reactivate", ReactivateAsync)
            .WithName("ReactivateOrganizationSequence")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Update);

        group.MapPost("/{sequenceId:guid}/archive", ArchiveAsync)
            .WithName("ArchiveOrganizationSequence")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationConfigurations.Archive);

        return endpoints;
    }

    private static async Task<IResult> GetListAsync(Guid organizationId, Guid? branchId, IMediator mediator,
        IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryOrganization(organizationId, tenant, context, out var orgId, out var failure)) return failure!;
        BranchId? branch = TryBranch(branchId, context, out failure);
        if (failure is not null) return failure;
        Result<IReadOnlyList<OrganizationSequenceListItem>> result = await mediator.Send(new GetOrganizationSequencesQuery(orgId, branch), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        return Results.Ok(result.Value.Select(x => mapper.Map<OrganizationSequenceListItem, OrganizationSequenceListItemContract>(x)).ToArray());
    }

    private static async Task<IResult> GetByIdAsync(Guid organizationId, Guid sequenceId, IMediator mediator,
        IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryIds(organizationId, sequenceId, tenant, context, out var orgId, out var id, out var failure)) return failure!;
        Result<OrganizationSequenceResponse> result = await mediator.Send(new GetOrganizationSequenceByIdQuery(orgId, id), ct);
        return result.IsSuccess
            ? Results.Ok(mapper.Map<OrganizationSequenceResponse, OrganizationSequenceResponseContract>(result.Value))
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CreateAsync(Guid organizationId, CreateOrganizationSequenceRequest request,
        IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryOrganization(organizationId, tenant, context, out var orgId, out var failure)) return failure!;
        BranchId? branch = TryBranch(request.BranchId, context, out failure);
        if (failure is not null) return failure;
        var model = new CreateOrganizationSequenceApiModel(orgId, branch, request.Scope, request.Code, request.Pattern,
            request.Padding, request.InitialValue, request.ResetPolicy);
        Result<OrganizationSequenceId> result = await mediator.Send(mapper.Map<CreateOrganizationSequenceApiModel, CreateOrganizationSequenceCommand>(model), ct);
        return result.IsSuccess
            ? Results.Created($"/api/organizations/{organizationId}/sequences/{result.Value.Value}", new { id = result.Value.Value })
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> ReserveAsync(Guid organizationId, ReserveOrganizationSequenceNumberRequest request,
        IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryOrganization(organizationId, tenant, context, out var orgId, out var failure)) return failure!;
        BranchId? branch = TryBranch(request.BranchId, context, out failure);
        if (failure is not null) return failure;
        var model = new ReserveOrganizationSequenceNumberApiModel(orgId, branch, request.Code);
        Result<string> result = await mediator.Send(mapper.Map<ReserveOrganizationSequenceNumberApiModel, ReserveOrganizationSequenceNumberCommand>(model), ct);
        return result.IsSuccess ? Results.Ok(new OrganizationSequenceNumberResponse(result.Value)) : result.Error.ToHttpResult(context);
    }

    private static Task<IResult> SuspendAsync(Guid organizationId, Guid sequenceId, ChangeOrganizationSequenceStatusRequest request,
        IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        ChangeStatusAsync(organizationId, sequenceId, request, mediator, mapper, tenant, context, ct, "suspend");

    private static Task<IResult> ReactivateAsync(Guid organizationId, Guid sequenceId, ChangeOrganizationSequenceStatusRequest request,
        IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        ChangeStatusAsync(organizationId, sequenceId, request, mediator, mapper, tenant, context, ct, "reactivate");

    private static Task<IResult> ArchiveAsync(Guid organizationId, Guid sequenceId, ChangeOrganizationSequenceStatusRequest request,
        IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        ChangeStatusAsync(organizationId, sequenceId, request, mediator, mapper, tenant, context, ct, "archive");

    private static async Task<IResult> ChangeStatusAsync(Guid organizationId, Guid sequenceId,
        ChangeOrganizationSequenceStatusRequest request, IMediator mediator, IObjectMapper mapper,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct, string action)
    {
        if (!TryIds(organizationId, sequenceId, tenant, context, out var orgId, out var id, out var failure)) return failure!;
        var model = new ChangeOrganizationSequenceStatusApiModel(orgId, id, request.ExpectedRevision);
        Result result = action switch
        {
            "suspend" => await mediator.Send(mapper.Map<ChangeOrganizationSequenceStatusApiModel, SuspendOrganizationSequenceCommand>(model), ct),
            "reactivate" => await mediator.Send(mapper.Map<ChangeOrganizationSequenceStatusApiModel, ReactivateOrganizationSequenceCommand>(model), ct),
            _ => await mediator.Send(mapper.Map<ChangeOrganizationSequenceStatusApiModel, ArchiveOrganizationSequenceCommand>(model), ct)
        };
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static bool TryIds(Guid rawOrganizationId, Guid rawSequenceId, ICurrentTenant tenant, HttpContext context,
        out OrganizationId organizationId, out OrganizationSequenceId sequenceId, out IResult? failure)
    {
        sequenceId = default;
        if (!TryOrganization(rawOrganizationId, tenant, context, out organizationId, out failure)) return false;
        if (rawSequenceId == Guid.Empty)
        {
            failure = OrganizationSequenceErrors.EmptyId.ToHttpResult(context);
            return false;
        }

        sequenceId = new OrganizationSequenceId(rawSequenceId);
        return true;
    }

    private static bool TryOrganization(Guid rawId, ICurrentTenant tenant, HttpContext context,
        out OrganizationId organizationId, out IResult? failure)
    {
        organizationId = default; failure = null;
        if (rawId == Guid.Empty)
        {
            failure = OrganizationSequenceErrors.EmptyOrganizationId.ToHttpResult(context);
            return false;
        }

        organizationId = new OrganizationId(rawId);

        if (!tenant.HasTenant || tenant.OrganizationId != organizationId)
        {
            failure = Error.Forbidden(
                    "OrganizationSequences.Tenant.Forbidden",
                    "errors.organizationSequence.tenant.forbidden")
                .ToHttpResult(context);
            return false;
        }

        return true;
    }

    private static BranchId? TryBranch(Guid? rawId, HttpContext context, out IResult? failure)
    {
        failure = null;
        if (rawId is null) return null;
        if (rawId.Value == Guid.Empty)
        {
            failure = OrganizationSequenceErrors.EmptyBranchId.ToHttpResult(context);
            return null;
        }

        return new BranchId(rawId.Value);
    }
}
