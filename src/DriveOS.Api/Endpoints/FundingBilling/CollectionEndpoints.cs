using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.Collections.DetectOverdue;
using DriveOS.Modules.FundingBilling.Application.Collections.Read;
using DriveOS.Modules.FundingBilling.Application.Collections.Reminders;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Api.Endpoints.FundingBilling;
public sealed record RequestPaymentReminderRequest(string TargetType, Guid TargetId);
public static class CollectionEndpoints
{
    public static IEndpointRouteBuilder MapCollectionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/finance/collections").WithTags("Funding & Billing - Collections");
        group.MapGet("/overdue", ListOverdueAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CollectionsRead);
        group.MapPost("/detect-overdue", DetectOverdueAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CollectionsManage);
        group.MapPost("/reminders", RequestReminderAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CollectionsManage);
        group.MapPost("/reminders/dispatch-pending", DispatchPendingRemindersAsync).RequireAuthorization(DriveOsPermissionCodes.Finance.CollectionsManage);
        return endpoints;
    }
    private static async Task<IResult> ListOverdueAsync(IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        var result = await mediator.Send(new GetOverdueItemsQuery(tenant.OrganizationId.Value, DateOnly.FromDateTime(DateTime.UtcNow)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }
    private static async Task<IResult> DetectOverdueAsync(IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var userId, out var error)) return error!;
        var result = await mediator.Send(new DetectOverdueCommand(organizationId, DateOnly.FromDateTime(DateTime.UtcNow), userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }
    private static async Task<IResult> RequestReminderAsync(RequestPaymentReminderRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var userId, out var error)) return error!;
        if (!Enum.TryParse<PaymentReminderTargetType>(request.TargetType, true, out var targetType)) return Results.Problem(statusCode: 400, title: "errors.fundingBilling.reminder.target.invalid");
        var result = await mediator.Send(new RequestPaymentReminderCommand(organizationId, targetType, request.TargetId, userId), ct);
        return result.IsSuccess ? Results.Accepted($"/api/finance/collections/overdue", new { reminderId = result.Value.Value }) : Problem(result.Error);
    }
    private static async Task<IResult> DispatchPendingRemindersAsync(int? take, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out var organizationId, out var userId, out var error)) return error!;
        var result = await mediator.Send(new DispatchPendingPaymentRemindersCommand(organizationId, take ?? 50, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }
    private static bool TryContext(ICurrentTenant tenant, ICurrentUser user, out OrganizationId organizationId, out UserId userId, out IResult? error)
    {
        organizationId = default; userId = default; error = null;
        if (!tenant.HasTenant || tenant.OrganizationId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentTenant.required"); return false; }
        if (user.UserId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentUser.required"); return false; }
        organizationId = tenant.OrganizationId.Value; userId = user.UserId.Value; return true;
    }
    private static IResult Problem(Error error) => Results.Problem(statusCode: error.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: error.Code, detail: error.MessageKey, extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
