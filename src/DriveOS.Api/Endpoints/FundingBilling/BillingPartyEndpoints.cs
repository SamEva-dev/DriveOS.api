using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.BillingParties.Manage;
using DriveOS.Modules.FundingBilling.Application.BillingParties.Read;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record AddBillingPartyRequest(Guid? PersonId, Guid? OrganizationId, string Role, decimal? MaximumAmount, DateOnly EffectiveFrom, DateOnly? EffectiveTo, int Priority = 10, bool IsPrimary = false);
public sealed record EndBillingPartyRequest(string Reason);

public static class BillingPartyEndpoints
{
    public static IEndpointRouteBuilder MapBillingPartyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/finance").WithTags("Funding & Billing - Financial parties");
        group.MapGet("/billing-accounts/{billingAccountId:guid}/parties", ListAsync).Produces<IReadOnlyCollection<BillingPartyResponse>>().RequireAuthorization(DriveOsPermissionCodes.Finance.BillingPartiesRead);
        group.MapPost("/billing-accounts/{billingAccountId:guid}/parties", AddAsync).Produces<Guid>(201).RequireAuthorization(DriveOsPermissionCodes.Finance.BillingPartiesManage);
        group.MapPost("/billing-parties/{billingPartyId:guid}/end", EndAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.BillingPartiesManage);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(Guid billingAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        var result = await mediator.Send(new GetBillingPartiesQuery(tenant.OrganizationId.Value, new BillingAccountId(billingAccountId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> AddAsync(Guid billingAccountId, AddBillingPartyRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var userId, out var error)) return error!;
        if (!Enum.TryParse<BillingPartyRole>(request.Role, true, out var role) || !Enum.IsDefined(role)) return Results.Problem(statusCode: 400, title: BillingPartyErrors.InvalidRole.Code, detail: BillingPartyErrors.InvalidRole.MessageKey);
        var result = await mediator.Send(new AddBillingPartyCommand(organizationId, new BillingAccountId(billingAccountId), request.PersonId, request.OrganizationId, role, request.MaximumAmount, request.EffectiveFrom, request.EffectiveTo, request.Priority, request.IsPrimary, userId), ct);
        return result.IsSuccess ? Results.Created($"/api/finance/billing-parties/{result.Value.Value}", result.Value.Value) : Problem(result.Error);
    }

    private static async Task<IResult> EndAsync(Guid billingPartyId, EndBillingPartyRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var userId, out var error)) return error!;
        var result = await mediator.Send(new EndBillingPartyCommand(organizationId, new BillingPartyId(billingPartyId), request.Reason, userId), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static bool TryContext(ICurrentTenant tenant, ICurrentUser user, out OrganizationId organizationId, out UserId userId, out IResult? error)
    {
        organizationId = default; userId = default; error = null;
        if (!tenant.HasTenant || tenant.OrganizationId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentTenant.required"); return false; }
        if (user.UserId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentUser.required"); return false; }
        organizationId = tenant.OrganizationId.Value; userId = user.UserId.Value; return true;
    }
    private static IResult Problem(Error e) => Results.Problem(statusCode: e.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: e.Code, detail: e.MessageKey, extensions: new Dictionary<string, object?> { { "code", e.Code } });
}
