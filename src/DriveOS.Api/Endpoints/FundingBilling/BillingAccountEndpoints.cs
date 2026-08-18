using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
using DriveOS.Modules.FundingBilling.Application.BillingAccounts.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;
public sealed record CreateBillingAccountRequest(Guid StudentId, string Currency);
public static class BillingAccountEndpoints
{
    public static IEndpointRouteBuilder MapBillingAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/finance/billing-accounts").WithTags("Funding & Billing - Accounts");
        group.MapPost("", CreateAsync).Produces<Guid>(StatusCodes.Status201Created).RequireAuthorization(DriveOsPermissionCodes.Finance.BillingAccountsCreate);
        group.MapGet("/{billingAccountId:guid}", GetByIdAsync).Produces<BillingAccountResponse>().RequireAuthorization(DriveOsPermissionCodes.Finance.BillingAccountsRead);
        group.MapGet("/by-student/{studentId:guid}", GetByStudentAsync).Produces<BillingAccountResponse>().RequireAuthorization(DriveOsPermissionCodes.Finance.BillingAccountsRead);
        return endpoints;
    }
    private static async Task<IResult> CreateAsync(CreateBillingAccountRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        if (user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<BillingAccountId> result = await mediator.Send(new CreateBillingAccountCommand(tenant.OrganizationId.Value, new PersonId(request.StudentId), request.Currency, user.UserId.Value), ct);
        return result.IsSuccess ? Results.Created($"/api/finance/billing-accounts/{result.Value.Value}", result.Value.Value) : Problem(result.Error);
    }
    private static async Task<IResult> GetByIdAsync(Guid billingAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<BillingAccountResponse> result = await mediator.Send(new GetBillingAccountQuery(tenant.OrganizationId.Value, new BillingAccountId(billingAccountId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }
    private static async Task<IResult> GetByStudentAsync(Guid studentId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<BillingAccountResponse> result = await mediator.Send(new GetStudentBillingAccountQuery(tenant.OrganizationId.Value, new PersonId(studentId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }
    private static IResult Problem(Error error) => Results.Problem(statusCode: error.Type switch { ErrorType.NotFound => 404, ErrorType.Conflict => 409, ErrorType.Validation => 400, _ => 400 }, title: error.Code, detail: error.MessageKey, extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
