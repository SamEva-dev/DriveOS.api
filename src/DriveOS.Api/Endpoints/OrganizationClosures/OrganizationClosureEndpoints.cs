using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Create;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.GetById;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.GetByOrganization;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.GetReadiness;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Transition;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.OrganizationClosures;

public static class OrganizationClosureEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationClosureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder organizationGroup = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/closures")
            .WithTags("Organization closures");

        organizationGroup.MapGet("/", ListAsync)
            .WithName("GetOrganizationClosures")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationClosures.Read);

        organizationGroup.MapPost("/", CreateAsync)
            .WithName("CreateOrganizationClosure")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationClosures.Create);

        RouteGroupBuilder closureGroup = endpoints
            .MapGroup("/api/organization-closures/{closureId:guid}")
            .WithTags("Organization closures");

        closureGroup.MapGet("/", GetAsync)
            .WithName("GetOrganizationClosure")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationClosures.Read);

        closureGroup.MapGet("/readiness", GetReadinessAsync)
            .WithName("GetOrganizationClosureReadiness")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationClosures.Read);

        MapAction(closureGroup, "submit", OrganizationClosureAction.Submit, DriveOsPermissionCodes.OrganizationClosures.Submit);
        MapAction(closureGroup, "approve", OrganizationClosureAction.Approve, DriveOsPermissionCodes.OrganizationClosures.Approve);
        MapAction(closureGroup, "reject", OrganizationClosureAction.Reject, DriveOsPermissionCodes.OrganizationClosures.Reject);
        MapAction(closureGroup, "cancel", OrganizationClosureAction.Cancel, DriveOsPermissionCodes.OrganizationClosures.Cancel);
        MapAction(closureGroup, "complete", OrganizationClosureAction.Complete, DriveOsPermissionCodes.OrganizationClosures.Complete);

        closureGroup.MapPost("/schedule", ScheduleAsync)
            .WithName("ScheduleOrganizationClosure")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationClosures.Schedule);

        return endpoints;
    }

    private static void MapAction(RouteGroupBuilder group, string route, OrganizationClosureAction action, string permission) =>
        group.MapPost($"/{route}", async (Guid closureId, OrganizationClosureActionRequest request, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            Result result = await mediator.Send(new TransitionOrganizationClosureCommand(new OrganizationClosureId(closureId), action, request.Comment, null), ct);
            return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
        }).RequireAuthorization(permission);

    private static async Task<IResult> ListAsync(Guid organizationId, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!IsCurrentTenant(organizationId, tenant)) return Results.Forbid();
        Result<IReadOnlyList<OrganizationClosureModel>> result = await mediator.Send(new GetOrganizationClosuresQuery(new OrganizationId(organizationId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value.Select(Map)) : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CreateAsync(Guid organizationId, CreateOrganizationClosureRequest request, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!IsCurrentTenant(organizationId, tenant)) return Results.Forbid();
        var command = new CreateOrganizationClosureCommand(new OrganizationId(organizationId), request.ReasonCode, request.ReasonDetails, request.RequestedEffectiveAtUtc, request.DataDisposition, request.RetentionUntilUtc);
        Result<OrganizationClosureId> result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/organization-closures/{result.Value.Value}", new { id = result.Value.Value })
            : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> GetAsync(Guid closureId, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        Result<OrganizationClosureModel> result = await mediator.Send(new GetOrganizationClosureQuery(new OrganizationClosureId(closureId)), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        if (!IsCurrentTenant(result.Value.OrganizationId.Value, tenant)) return Results.Forbid();
        return Results.Ok(Map(result.Value));
    }

    private static async Task<IResult> GetReadinessAsync(Guid closureId, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        Result<OrganizationClosureReadinessModel> result = await mediator.Send(new GetOrganizationClosureReadinessQuery(new OrganizationClosureId(closureId)), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        if (!IsCurrentTenant(result.Value.OrganizationId.Value, tenant)) return Results.Forbid();
        return Results.Ok(new OrganizationClosureReadinessResponse(result.Value.OrganizationId.Value, result.Value.CanClose,
            result.Value.Requirements.Select(MapRequirement).ToArray(), result.Value.BlockingRequirements.Select(MapRequirement).ToArray()));
    }

    private static async Task<IResult> ScheduleAsync(Guid closureId, ScheduleOrganizationClosureRequest request, IMediator mediator, HttpContext context, CancellationToken ct)
    {
        Result result = await mediator.Send(new TransitionOrganizationClosureCommand(new OrganizationClosureId(closureId), OrganizationClosureAction.Schedule, null, request.ScheduledAtUtc), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static bool IsCurrentTenant(Guid organizationId, ICurrentTenant tenant) => tenant.OrganizationId?.Value == organizationId;
    private static OrganizationClosureResponse Map(OrganizationClosureModel x) => new(x.Id.Value, x.OrganizationId.Value, x.ReasonCode.ToString(), x.ReasonDetails, x.RequestedEffectiveAtUtc, x.DataDisposition.ToString(), x.RetentionUntilUtc, x.Status.ToString(), x.Revision, x.CreatedAtUtc, x.ReviewedAtUtc, x.ScheduledAtUtc, x.CompletedAtUtc, x.CancelledAtUtc, x.DecisionComment);
    private static OrganizationClosureRequirementResponse MapRequirement(OrganizationClosureRequirementModel x) => new(x.Code, x.IsSatisfied, x.Severity, x.MessageKey, x.Parameters);
}
