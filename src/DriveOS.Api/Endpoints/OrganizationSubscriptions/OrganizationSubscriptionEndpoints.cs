using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CancelOrganizationSubscription;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeOrganizationSubscriptionPlan;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeStatus;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckEntitlement;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckLimit;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CreateOrganizationSubscription;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.GetOrganizationSubscription;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.OrganizationSubscriptions;

public static class OrganizationSubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationSubscriptionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/organizations/{organizationId:guid}/subscription")
            .WithTags("Organization subscriptions");

        group.MapGet("/", GetAsync)
            .WithName("GetOrganizationSubscription")
            .Produces<OrganizationSubscriptionResponseContract>()
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Read);

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrganizationSubscription")
            .Produces(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Create);

        group.MapPost("/change-plan", ChangePlanAsync)
            .WithName("ChangeOrganizationSubscriptionPlan")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.ChangePlan);

        group.MapPost("/activate", (Guid organizationId, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
                ChangeStatusAsync(organizationId, SubscriptionStatus.Active, request, mediator, mapper, tenant, user, context, ct))
            .WithName("ActivateOrganizationSubscription")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Activate);

        group.MapPost("/mark-past-due", (Guid organizationId, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
                ChangeStatusAsync(organizationId, SubscriptionStatus.PastDue, request, mediator, mapper, tenant, user, context, ct))
            .WithName("MarkOrganizationSubscriptionPastDue")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.MarkPastDue);

        group.MapPost("/restrict", (Guid organizationId, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
                ChangeStatusAsync(organizationId, SubscriptionStatus.Restricted, request, mediator, mapper, tenant, user, context, ct))
            .WithName("RestrictOrganizationSubscription")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Restrict);

        group.MapPost("/suspend", (Guid organizationId, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
                ChangeStatusAsync(organizationId, SubscriptionStatus.Suspended, request, mediator, mapper, tenant, user, context, ct))
            .WithName("SuspendOrganizationSubscription")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Suspend);

        group.MapPost("/expire", (Guid organizationId, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
                ChangeStatusAsync(organizationId, SubscriptionStatus.Expired, request, mediator, mapper, tenant, user, context, ct))
            .WithName("ExpireOrganizationSubscription")
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Expire);

        group.MapPost("/cancel", CancelAsync)
            .WithName("CancelOrganizationSubscription")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.Cancel);

        group.MapGet("/entitlements/{entitlementCode}", CheckEntitlementAsync)
            .WithName("CheckOrganizationSubscriptionEntitlement")
            .Produces<OrganizationEntitlementCheckResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.ReadEntitlements);

        group.MapGet("/limits/{limitCode}", CheckLimitAsync)
            .WithName("CheckOrganizationSubscriptionLimit")
            .Produces<OrganizationLimitCheckResponseContract>()
            .RequireAuthorization(DriveOsPermissionCodes.OrganizationSubscriptions.ReadLimits);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(Guid organizationId, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        Result<OrganizationSubscriptionResponse> result = await mediator.Send(new GetOrganizationSubscriptionQuery(id), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        return Results.Ok(mapper.Map<OrganizationSubscriptionResponse, OrganizationSubscriptionResponseContract>(result.Value));
    }

    private static async Task<IResult> CreateAsync(Guid organizationId, CreateOrganizationSubscriptionRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        var model = new CreateOrganizationSubscriptionApiModel(id, request.PlanCode, request.Status, request.BillingCycle, request.CurrentPeriodStartsAtUtc, request.CurrentPeriodEndsAtUtc, request.TrialStartsAtUtc, request.TrialEndsAtUtc, request.ExternalProvider, request.ExternalSubscriptionId);
        CreateOrganizationSubscriptionCommand command = mapper.Map<CreateOrganizationSubscriptionApiModel, CreateOrganizationSubscriptionCommand>(model);
        Result<OrganizationSubscriptionId> result = await mediator.Send(command, ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        return Results.Created($"/api/organizations/{organizationId}/subscription", new { id = result.Value.Value });
    }

    private static async Task<IResult> ChangePlanAsync(Guid organizationId, ChangeOrganizationSubscriptionPlanRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        if (!TryGetActor(user, context, out UserId actor, out failure)) return failure!;
        var model = new ChangeOrganizationSubscriptionPlanApiModel(id, request.PlanCode, request.EntitlementCodes, request.Limits, request.ExpectedVersion, request.Reason, actor);
        Result result = await mediator.Send(mapper.Map<ChangeOrganizationSubscriptionPlanApiModel, ChangeOrganizationSubscriptionPlanCommand>(model), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> ChangeStatusAsync(Guid organizationId, SubscriptionStatus targetStatus, ChangeOrganizationSubscriptionStatusRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        if (!TryGetActor(user, context, out UserId actor, out failure)) return failure!;
        var model = new ChangeOrganizationSubscriptionStatusApiModel(id, targetStatus, request.PeriodStartsAtUtc, request.PeriodEndsAtUtc, request.ExpectedVersion, request.Reason, actor);
        Result result = await mediator.Send(mapper.Map<ChangeOrganizationSubscriptionStatusApiModel, ChangeOrganizationSubscriptionStatusCommand>(model), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CancelAsync(Guid organizationId, CancelOrganizationSubscriptionRequest request, IMediator mediator, IObjectMapper mapper, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        if (!TryGetActor(user, context, out UserId actor, out failure)) return failure!;
        var model = new CancelOrganizationSubscriptionApiModel(id, DateTimeOffset.UtcNow, request.EffectiveAtUtc, request.Reason, actor, request.ExpectedVersion);
        Result result = await mediator.Send(mapper.Map<CancelOrganizationSubscriptionApiModel, CancelOrganizationSubscriptionCommand>(model), ct);
        return result.IsSuccess ? Results.NoContent() : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CheckEntitlementAsync(Guid organizationId, string entitlementCode, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        Result<bool> result = await mediator.Send(new CheckOrganizationEntitlementQuery(id, entitlementCode), ct);
        return result.IsSuccess ? Results.Ok(new OrganizationEntitlementCheckResponse(entitlementCode, result.Value)) : result.Error.ToHttpResult(context);
    }

    private static async Task<IResult> CheckLimitAsync(Guid organizationId, string limitCode, long currentUsage, long requestedIncrease, IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!TryGetScopedOrganizationId(organizationId, tenant, context, out OrganizationId id, out IResult? failure)) return failure!;
        Result<OrganizationLimitCheckResponse> result = await mediator.Send(new CheckOrganizationLimitQuery(id, limitCode, currentUsage, requestedIncrease), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        return Results.Ok(new OrganizationLimitCheckResponseContract(limitCode, (int)result.Value.Availability, result.Value.Limit, result.Value.CurrentUsage, result.Value.RequestedIncrease));
    }

    private static bool TryGetScopedOrganizationId(
    Guid rawOrganizationId,
    ICurrentTenant currentTenant,
    HttpContext httpContext,
    out OrganizationId organizationId,
    out IResult? failure)
    {
        organizationId = new OrganizationId(rawOrganizationId);

        if (organizationId.IsEmpty)
        {
            failure = Error.Validation(
                    "errors.organizationSubscription.organizationId.required",
                    "Organization id is required.")
                .ToHttpResult(httpContext);

            return false;
        }

        failure = EnsureTenantScope(
            organizationId,
            currentTenant,
            httpContext);

        return failure is null;
    }

    private static bool TryGetActor(ICurrentUser user, HttpContext context, out UserId actor, out IResult? failure)
    {
        actor = user.UserId ?? UserId.Empty;
        if (actor.IsEmpty)
        {
            failure = Error.Forbidden("errors.organizationSubscription.actor.required", "An authenticated user identifier is required.").ToHttpResult(context);
            return false;
        }
        failure = null;
        return true;
    }

    private static IResult? EnsureTenantScope(
    OrganizationId requestedOrganizationId,
    ICurrentTenant currentTenant,
    HttpContext httpContext)
    {
        if (!currentTenant.HasTenant)
        {
            return null;
        }

        return currentTenant.OrganizationId == requestedOrganizationId
            ? null
            : Error.Forbidden(
                    "errors.organizationSubscription.tenantScopeMismatch",
                    "The requested organization is outside the current tenant scope.")
                .ToHttpResult(httpContext);
    }
}
