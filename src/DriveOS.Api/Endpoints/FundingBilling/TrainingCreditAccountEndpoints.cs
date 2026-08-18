using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Create;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Read;
using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Manage;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record CreateTrainingCreditAccountRequest(string CreditType, DateOnly? ExpirationDate);
public sealed record RecordTrainingCreditMovementRequest(decimal Quantity, string Reference, string? Reason);

public static class TrainingCreditAccountEndpoints
{
    public static IEndpointRouteBuilder MapTrainingCreditAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/finance").WithTags("Funding & Billing - Training credits");

        group.MapGet("/billing-accounts/{billingAccountId:guid}/training-credits", ListAsync)
            .Produces<IReadOnlyCollection<TrainingCreditAccountResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.TrainingCreditsRead);

        group.MapGet("/training-credit-accounts/{trainingCreditAccountId:guid}", GetAsync)
            .Produces<TrainingCreditAccountResponse>()
            .Produces(404)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.TrainingCreditsRead);

        group.MapPost("/billing-accounts/{billingAccountId:guid}/training-credits", CreateAsync)
            .Produces<Guid>(201)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.TrainingCreditsManage);

        group.MapGet("/training-credit-accounts/{trainingCreditAccountId:guid}/movements", ListMovementsAsync)
            .Produces<IReadOnlyCollection<TrainingCreditMovementResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.TrainingCreditsRead);

        MapOperation(group, "purchase", TrainingCreditOperation.Purchase);
        MapOperation(group, "reserve", TrainingCreditOperation.Reserve);
        MapOperation(group, "release", TrainingCreditOperation.Release);
        MapOperation(group, "consume", TrainingCreditOperation.Consume);
        MapOperation(group, "adjust", TrainingCreditOperation.Adjust);

        return endpoints;
    }

    private static async Task<IResult> ListAsync(Guid billingAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        Result<IReadOnlyCollection<TrainingCreditAccountResponse>> result = await mediator.Send(
            new GetTrainingCreditAccountsQuery(tenant.OrganizationId.Value, new BillingAccountId(billingAccountId)), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetAsync(Guid trainingCreditAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");

        Result<TrainingCreditAccountResponse> result = await mediator.Send(
            new GetTrainingCreditAccountQuery(tenant.OrganizationId.Value, new TrainingCreditAccountId(trainingCreditAccountId)), ct);

        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> CreateAsync(Guid billingAccountId, CreateTrainingCreditAccountRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        if (user.UserId is null)
            return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Result<TrainingCreditAccountId> result = await mediator.Send(new CreateTrainingCreditAccountCommand(
            tenant.OrganizationId.Value,
            new BillingAccountId(billingAccountId),
            request.CreditType,
            request.ExpirationDate,
            user.UserId.Value), ct);

        return result.IsSuccess
            ? Results.Created($"/api/finance/training-credit-accounts/{result.Value.Value}", result.Value.Value)
            : Problem(result.Error);
    }


    private static void MapOperation(RouteGroupBuilder group, string route, TrainingCreditOperation operation)
    {
        group.MapPost($"/training-credit-accounts/{{trainingCreditAccountId:guid}}/{route}",
                (Guid trainingCreditAccountId, RecordTrainingCreditMovementRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct) =>
                    RecordMovementAsync(trainingCreditAccountId, operation, request, mediator, tenant, user, ct))
            .Produces<Guid>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.TrainingCreditsManage);
    }

    private static async Task<IResult> ListMovementsAsync(Guid trainingCreditAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<IReadOnlyCollection<TrainingCreditMovementResponse>> result = await mediator.Send(
            new GetTrainingCreditMovementsQuery(tenant.OrganizationId.Value, new TrainingCreditAccountId(trainingCreditAccountId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> RecordMovementAsync(Guid trainingCreditAccountId, TrainingCreditOperation operation,
        RecordTrainingCreditMovementRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        if (user.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");
        Result<TrainingCreditMovementId> result = await mediator.Send(new RecordTrainingCreditMovementCommand(
            tenant.OrganizationId.Value, new TrainingCreditAccountId(trainingCreditAccountId), operation, request.Quantity, request.Reference, request.Reason, user.UserId.Value), ct);
        return result.IsSuccess ? Results.Ok(result.Value.Value) : Problem(result.Error);
    }

    private static IResult Problem(Error error) => Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Validation => 400,
            _ => 400
        },
        title: error.Code,
        detail: error.MessageKey,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
