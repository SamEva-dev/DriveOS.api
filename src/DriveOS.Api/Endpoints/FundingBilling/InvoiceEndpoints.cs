using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.Invoices.Create;
using DriveOS.Modules.FundingBilling.Application.Invoices.Issue;
using DriveOS.Modules.FundingBilling.Application.Invoices.Lines;
using DriveOS.Modules.FundingBilling.Application.Invoices.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record CreateInvoiceLineRequest(
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate);

public sealed record CreateInvoiceRequest(Guid BillingAccountId, IReadOnlyCollection<CreateInvoiceLineRequest> Lines);
public sealed record AddInvoiceLineRequest(string Description, decimal Quantity, string Unit, decimal UnitPrice, decimal DiscountAmount, decimal TaxRate);
public sealed record IssueInvoiceRequest(DateOnly IssueDate, DateOnly DueDate);

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/finance/invoices").WithTags("Funding & Billing - Invoices");

        group.MapPost("", CreateAsync)
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesCreate);

        group.MapGet("/{invoiceId:guid}", GetAsync)
            .Produces<InvoiceResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesRead);

        group.MapGet("/by-billing-account/{billingAccountId:guid}", ListByBillingAccountAsync)
            .Produces<IReadOnlyCollection<InvoiceResponse>>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesRead);

        group.MapPost("/{invoiceId:guid}/lines", AddLineAsync)
            .Produces<Guid>(StatusCodes.Status201Created)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesManageDraft);

        group.MapDelete("/{invoiceId:guid}/lines/{lineId:guid}", RemoveLineAsync)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesManageDraft);

        group.MapPost("/{invoiceId:guid}/issue", IssueAsync)
            .Produces<IssueInvoiceResponse>()
            .RequireAuthorization(DriveOsPermissionCodes.Finance.InvoicesIssue);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(CreateInvoiceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        CreateInvoiceCommand command = new(
            organizationId,
            new BillingAccountId(request.BillingAccountId),
            request.Lines.Select(x => new CreateInvoiceLineInput(x.Description, x.Quantity, x.Unit, x.UnitPrice, x.DiscountAmount, x.TaxRate)).ToArray(),
            userId);
        Result<InvoiceId> result = await mediator.Send(command, ct);
        return result.IsSuccess ? Results.Created($"/api/finance/invoices/{result.Value.Value}", result.Value.Value) : Problem(result.Error);
    }

    private static async Task<IResult> AddLineAsync(Guid invoiceId, AddInvoiceLineRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result<InvoiceLineId> result = await mediator.Send(new AddInvoiceLineCommand(
            organizationId, new InvoiceId(invoiceId), request.Description, request.Quantity, request.Unit,
            request.UnitPrice, request.DiscountAmount, request.TaxRate, userId), ct);
        return result.IsSuccess ? Results.Created($"/api/finance/invoices/{invoiceId}/lines/{result.Value.Value}", result.Value.Value) : Problem(result.Error);
    }

    private static async Task<IResult> RemoveLineAsync(Guid invoiceId, Guid lineId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result result = await mediator.Send(new RemoveInvoiceLineCommand(organizationId, new InvoiceId(invoiceId), new InvoiceLineId(lineId), userId), ct);
        return result.IsSuccess ? Results.NoContent() : Problem(result.Error);
    }

    private static async Task<IResult> IssueAsync(Guid invoiceId, IssueInvoiceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryContext(tenant, user, out OrganizationId organizationId, out UserId userId, out IResult? error)) return error!;
        Result<IssueInvoiceResponse> result = await mediator.Send(new IssueInvoiceCommand(organizationId, new InvoiceId(invoiceId), request.IssueDate, request.DueDate, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> GetAsync(Guid invoiceId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<InvoiceResponse> result = await mediator.Send(new GetInvoiceQuery(tenant.OrganizationId.Value, new InvoiceId(invoiceId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
    }

    private static async Task<IResult> ListByBillingAccountAsync(Guid billingAccountId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return Results.Problem(statusCode: 401, title: "errors.currentTenant.required");
        Result<IReadOnlyCollection<InvoiceResponse>> result = await mediator.Send(new GetBillingAccountInvoicesQuery(tenant.OrganizationId.Value, new BillingAccountId(billingAccountId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);
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
