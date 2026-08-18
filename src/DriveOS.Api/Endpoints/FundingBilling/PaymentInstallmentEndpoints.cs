using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.Installments.Create;
using DriveOS.Modules.FundingBilling.Application.Installments.Manage;
using DriveOS.Modules.FundingBilling.Application.Installments.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record CreatePaymentInstallmentRequest(
    DateOnly DueDate,
    decimal ExpectedAmount,
    Guid? FinancingPersonId,
    Guid? FinancingOrganizationId);

public sealed record CreatePaymentScheduleRequest(IReadOnlyCollection<CreatePaymentInstallmentRequest> Installments);
public sealed record ReschedulePaymentInstallmentRequest(DateOnly DueDate, string Reason);
public sealed record PaymentInstallmentReasonRequest(string Reason);

public static class PaymentInstallmentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentInstallmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/finance").WithTags("Funding & Billing - Payment schedule");

        group.MapPost("/billing-accounts/{billingAccountId:guid}/installments", CreateScheduleAsync)
            .Produces<IReadOnlyCollection<Guid>>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsManage);

        group.MapGet("/billing-accounts/{billingAccountId:guid}/installments", ListAsync)
            .Produces<IReadOnlyCollection<PaymentInstallmentResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsRead);

        group.MapGet("/installments/{installmentId:guid}", GetAsync)
            .Produces<PaymentInstallmentResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsRead);

        group.MapPost("/installments/{installmentId:guid}/reschedule", RescheduleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsManage);

        group.MapPost("/installments/{installmentId:guid}/cancel", CancelAsync)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsManage);

        group.MapPost("/installments/{installmentId:guid}/waive", WaiveAsync)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InstallmentsManage);

        return endpoints;
    }

    private static async Task<IResult> CreateScheduleAsync(Guid billingAccountId, CreatePaymentScheduleRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;

        var inputs = request.Installments.Select(x => new CreatePaymentInstallmentInput(
            x.DueDate,
            x.ExpectedAmount,
            x.FinancingPersonId,
            x.FinancingOrganizationId)).ToArray();

        Result<IReadOnlyCollection<PaymentInstallmentId>> result = await mediator.Send(
            new CreatePaymentScheduleCommand(organizationId, new BillingAccountId(billingAccountId), inputs, userId), ct);

        return result.IsSuccess
            ? Results.Created($"/api/finance/billing-accounts/{billingAccountId}/installments", result.Value.Select(x => x.Value).ToArray())
            : Problem(result.Error);
    }

    private static async Task<IResult> ListAsync(Guid billingAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<IReadOnlyCollection<PaymentInstallmentResponse>> result = await mediator.Send(
            new GetBillingAccountInstallmentsQuery(tenant.OrganizationId.Value, new BillingAccountId(billingAccountId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetAsync(Guid installmentId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<PaymentInstallmentResponse> result = await mediator.Send(
            new GetPaymentInstallmentQuery(tenant.OrganizationId.Value, new PaymentInstallmentId(installmentId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> RescheduleAsync(Guid installmentId, ReschedulePaymentInstallmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result result = await mediator.Send(new ReschedulePaymentInstallmentCommand(
            organizationId, new PaymentInstallmentId(installmentId), request.DueDate, request.Reason, userId), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> CancelAsync(Guid installmentId, PaymentInstallmentReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result result = await mediator.Send(new CancelPaymentInstallmentCommand(
            organizationId, new PaymentInstallmentId(installmentId), request.Reason, userId), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> WaiveAsync(Guid installmentId, PaymentInstallmentReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result result = await mediator.Send(new WaivePaymentInstallmentCommand(
            organizationId, new PaymentInstallmentId(installmentId), request.Reason, userId), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static bool TryContext(ICurrentTenant tenant, ICurrentUser user, out OrganizationId organizationId, out UserId userId, out IResult? error)
    {
        organizationId = default;
        userId = default;
        error = null;
        if (!tenant.HasTenant || tenant.OrganizationId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentTenant.required"); return false; }
        if (user.UserId is null) { error = Results.Problem(statusCode: 401, title: "errors.currentUser.required"); return false; }
        organizationId = tenant.OrganizationId.Value;
        userId = user.UserId.Value;
        return true;
    }

    private static IResult Problem(Error error) => Results.Problem(
        statusCode: error.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 },
        title: error.Code,
        detail: error.MessageKey,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
