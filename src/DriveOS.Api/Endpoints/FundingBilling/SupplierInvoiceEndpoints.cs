using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Application.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

internal static class SupplierInvoiceEndpoints
{
    internal static IEndpointRouteBuilder MapSupplierInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/finance/organizations/{organizationId:guid}/supplier-invoices")
            .WithTags("Finance - Supplier Invoices");

        g.MapPost("/{supplierInvoiceId:guid}/match",Match)
            .RequireAuthorization("Finance.SupplierInvoices.Match");

        g.MapPost("/{supplierInvoiceId:guid}/approve-operational",ApproveOperational)
            .RequireAuthorization("Finance.SupplierInvoices.ApproveOperational");

        g.MapPost("/{supplierInvoiceId:guid}/approve-financial",ApproveFinancial)
            .RequireAuthorization("Finance.SupplierInvoices.ApproveFinancial");

        g.MapPost("/{supplierInvoiceId:guid}/payment-attempts",SchedulePayment)
            .RequireAuthorization("Finance.SupplierInvoices.SchedulePayment");

        g.MapPost("/{supplierInvoiceId:guid}/manual-payment",RecordManualPayment)
            .RequireAuthorization("Finance.SupplierInvoices.RecordManualPayment");

        g.MapPost("/{supplierInvoiceId:guid}/refunds",RecordRefund)
            .RequireAuthorization("Finance.SupplierInvoices.RefundPayment");

        g.MapPost("/payment-batches",ScheduleBatch)
            .RequireAuthorization("Finance.SupplierInvoices.BatchPayment");

        g.MapPost("/{supplierInvoiceId:guid}/payment-attempts/{attemptId:guid}/processing",PaymentProcessing)
            .RequireAuthorization("Finance.SupplierInvoices.SchedulePayment");

        g.MapPost("/{supplierInvoiceId:guid}/payment-attempts/{attemptId:guid}/paid",PaymentPaid)
            .RequireAuthorization("Finance.SupplierInvoices.SchedulePayment");

        g.MapPost("/{supplierInvoiceId:guid}/payment-attempts/{attemptId:guid}/failed",PaymentFailed)
            .RequireAuthorization("Finance.SupplierInvoices.SchedulePayment");

        g.MapPost("/{supplierInvoiceId:guid}/payment-attempts/{attemptId:guid}/cancel",CancelPayment)
            .RequireAuthorization("Finance.SupplierInvoices.SchedulePayment");

        g.MapPost("/{supplierInvoiceId:guid}/reject",Reject)
            .RequireAuthorization("Finance.SupplierInvoices.ApproveOperational");

        g.MapPost("/{supplierInvoiceId:guid}/dispute",Dispute)
            .RequireAuthorization("Finance.SupplierInvoices.ApproveOperational");

        return app;
    }

    private static Task<IResult> Match(Guid organizationId,Guid supplierInvoiceId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new MatchSupplierInvoiceCommand(new(supplierInvoiceId),new(organizationId),a),m,ct);

    private static Task<IResult> ApproveOperational(Guid organizationId,Guid supplierInvoiceId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new ApproveSupplierInvoiceOperationalCommand(new(supplierInvoiceId),new(organizationId),a),m,ct);

    private static Task<IResult> ApproveFinancial(Guid organizationId,Guid supplierInvoiceId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new ApproveSupplierInvoiceFinancialCommand(new(supplierInvoiceId),new(organizationId),a),m,ct);

    private static async Task<IResult> SchedulePayment(Guid organizationId,Guid supplierInvoiceId,ScheduleSupplierPaymentRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new SupplierPaymentAttemptId(Guid.NewGuid());
        var r=await m.Send(new ScheduleSupplierPaymentCommand(
            id,new(supplierInvoiceId),new(organizationId),q.Amount,q.ScheduledDate,
            q.PaymentMethod,q.BankReference,actor),ct);
        return r.IsSuccess?Results.Created($"/api/finance/organizations/{organizationId}/supplier-invoices/{supplierInvoiceId}/payment-attempts/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static Task<IResult> PaymentProcessing(Guid organizationId,Guid supplierInvoiceId,Guid attemptId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new MarkSupplierPaymentProcessingCommand(new(attemptId),new(organizationId),a),m,ct);

    private static async Task<IResult> PaymentPaid(Guid organizationId,Guid supplierInvoiceId,Guid attemptId,SupplierPaymentPaidRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new MarkSupplierPaymentPaidCommand(
            new(attemptId),new(organizationId),q.SettledAmount,q.SettledOn,q.ProviderReference,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> PaymentFailed(Guid organizationId,Guid supplierInvoiceId,Guid attemptId,SupplierPaymentFailedRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new MarkSupplierPaymentFailedCommand(new(attemptId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static Task<IResult> CancelPayment(Guid organizationId,Guid supplierInvoiceId,Guid attemptId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new CancelSupplierPaymentAttemptCommand(new(attemptId),new(organizationId),a),m,ct);


    private static async Task<IResult> RecordManualPayment(
        Guid organizationId,Guid supplierInvoiceId,RecordManualSupplierPaymentRequest q,
        IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new SupplierPaymentAttemptId(Guid.NewGuid());
        var r=await m.Send(new RecordManualSupplierPaymentCommand(
            id,new(supplierInvoiceId),new(organizationId),q.Amount,q.PaidOn,
            q.PaymentMethod,q.BankReference,q.ProviderReference,actor),ct);
        return r.IsSuccess
            ?Results.Created($"/api/finance/organizations/{organizationId}/supplier-invoices/{supplierInvoiceId}/payment-attempts/{id.Value}",new{id=id.Value})
            :Problem(r.Error);
    }

    private static async Task<IResult> RecordRefund(
        Guid organizationId,Guid supplierInvoiceId,RecordSupplierPaymentRefundRequest q,
        IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new SupplierPaymentRefundId(Guid.NewGuid());
        var r=await m.Send(new RecordSupplierPaymentRefundCommand(
            id,new(supplierInvoiceId),new(organizationId),q.Amount,q.Reason,q.Method,q.ProviderReference,actor),ct);
        return r.IsSuccess?Results.Created($"/api/finance/organizations/{organizationId}/supplier-invoices/{supplierInvoiceId}/refunds/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> ScheduleBatch(
        Guid organizationId,ScheduleSupplierPaymentBatchRequest q,
        IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new SupplierPaymentBatchId(Guid.NewGuid());
        var items=(q.Items??[]).Select(x=>new ScheduleSupplierPaymentBatchItem(new(x.SupplierInvoiceId),x.Amount)).ToArray();
        var r=await m.Send(new ScheduleSupplierPaymentBatchCommand(
            id,new(organizationId),q.ScheduledDate,q.PaymentMethod,q.BankReference,items,actor),ct);
        return r.IsSuccess?Results.Created($"/api/finance/organizations/{organizationId}/supplier-invoices/payment-batches/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> Reject(Guid organizationId,Guid supplierInvoiceId,SupplierInvoiceReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(new RejectSupplierInvoiceCommand(new(supplierInvoiceId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Dispute(Guid organizationId,Guid supplierInvoiceId,SupplierInvoiceReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(new DisputeSupplierInvoiceCommand(new(supplierInvoiceId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Mutate<T>(ICurrentUser u,Func<UserId,T> factory,IMediator m,CancellationToken ct) where T:ICommand
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(factory(actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record SupplierInvoiceReasonRequest(string Reason);
internal sealed record ScheduleSupplierPaymentRequest(
    decimal Amount,DateOnly ScheduledDate,string PaymentMethod,string? BankReference);
internal sealed record SupplierPaymentPaidRequest(
    decimal SettledAmount,DateOnly SettledOn,string? ProviderReference);
internal sealed record SupplierPaymentFailedRequest(string Reason);
internal sealed record RecordManualSupplierPaymentRequest(
    decimal Amount,DateOnly PaidOn,string PaymentMethod,string? BankReference,string? ProviderReference);
internal sealed record RecordSupplierPaymentRefundRequest(
    decimal Amount,string Reason,string Method,string? ProviderReference);
internal sealed record ScheduleSupplierPaymentBatchRequest(
    DateOnly ScheduledDate,string PaymentMethod,string? BankReference,ScheduleSupplierPaymentBatchItemRequest[] Items);
internal sealed record ScheduleSupplierPaymentBatchItemRequest(Guid SupplierInvoiceId,decimal Amount);
