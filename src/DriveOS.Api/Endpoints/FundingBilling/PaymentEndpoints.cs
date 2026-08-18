using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.Payments.Allocate;
using DriveOS.Modules.FundingBilling.Application.Payments.Create;
using DriveOS.Modules.FundingBilling.Application.Payments.Read;
using DriveOS.Modules.FundingBilling.Application.Payments.Record;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record CreatePaymentRequest(Guid BillingAccountId, Guid? PayerPersonId, Guid? PayerOrganizationId, decimal Amount, string PaymentMethod, string? ExternalReference);
public sealed record RecordPaymentReceivedRequest(string? ExternalReference);
public sealed record PaymentFailureRequest(string Reason);
public sealed record AllocatePaymentRequest(Guid? InvoiceId, Guid? InstallmentId, decimal Amount);

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/finance/payments").WithTags("Funding & Billing - Payments");
        group.MapPost("",CreateAsync).Produces<Guid>(201).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsCreate);
        group.MapGet("/{paymentId:guid}",GetAsync).Produces<PaymentResponse>().RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRead);
        group.MapGet("/by-billing-account/{billingAccountId:guid}",ListAsync).Produces<IReadOnlyCollection<PaymentResponse>>().RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRead);
        group.MapPost("/{paymentId:guid}/processing",ProcessingAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRecord);
        group.MapPost("/{paymentId:guid}/received",ReceivedAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRecord);
        group.MapPost("/{paymentId:guid}/failed",FailedAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRecord);
        group.MapPost("/{paymentId:guid}/cancel",CancelAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsRecord);
        group.MapPost("/{paymentId:guid}/allocations",AllocateAsync).Produces<Guid>(201).RequireAuthorization(DriveOsPermissionCodes.Finance.PaymentsAllocate);
        return endpoints;
    }
    static async Task<IResult> CreateAsync(CreatePaymentRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new CreatePaymentCommand(o,new BillingAccountId(r.BillingAccountId),r.PayerPersonId,r.PayerOrganizationId,r.Amount,r.PaymentMethod,r.ExternalReference,user),ct);return x.IsSuccess?Results.Created($"/api/finance/payments/{x.Value.Value}",x.Value.Value):Problem(x.Error);}
    static async Task<IResult> GetAsync(Guid paymentId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var x=await m.Send(new GetPaymentQuery(t.OrganizationId.Value,new PaymentId(paymentId)),ct);return x.IsSuccess?Results.Ok(x.Value):Problem(x.Error);}
    static async Task<IResult> ListAsync(Guid billingAccountId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var x=await m.Send(new GetBillingAccountPaymentsQuery(t.OrganizationId.Value,new BillingAccountId(billingAccountId)),ct);return x.IsSuccess?Results.Ok(x.Value):Problem(x.Error);}
    static async Task<IResult> ProcessingAsync(Guid paymentId,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new MarkPaymentProcessingCommand(o,new PaymentId(paymentId),user),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> ReceivedAsync(Guid paymentId,RecordPaymentReceivedRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new RecordPaymentReceivedCommand(o,new PaymentId(paymentId),r.ExternalReference,user),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> FailedAsync(Guid paymentId,PaymentFailureRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new MarkPaymentFailedCommand(o,new PaymentId(paymentId),r.Reason,user),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> CancelAsync(Guid paymentId,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new CancelPaymentCommand(o,new PaymentId(paymentId),user),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> AllocateAsync(Guid paymentId,AllocatePaymentRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var user,out var e))return e!;var x=await m.Send(new AllocatePaymentCommand(o,new PaymentId(paymentId),r.InvoiceId.HasValue?new InvoiceId(r.InvoiceId.Value):null,r.InstallmentId.HasValue?new PaymentInstallmentId(r.InstallmentId.Value):null,r.Amount,user),ct);return x.IsSuccess?Results.Created($"/api/finance/payments/{paymentId}/allocations/{x.Value.Value}",x.Value.Value):Problem(x.Error);}
    static bool Ctx(ICurrentTenant t,ICurrentUser u,out OrganizationId o,out UserId user,out IResult? e){o=default;user=default;e=null;if(!t.HasTenant||t.OrganizationId is null){e=Results.Problem(statusCode:401,title:"errors.currentTenant.required");return false;}if(u.UserId is null){e=Results.Problem(statusCode:401,title:"errors.currentUser.required");return false;}o=t.OrganizationId.Value;user=u.UserId.Value;return true;}
    static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},title:e.Code,detail:e.MessageKey,extensions:new Dictionary<string,object?>{{"code",e.Code}});
}
