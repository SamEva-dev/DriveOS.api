using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.Refunds.Create;
using DriveOS.Modules.FundingBilling.Application.Refunds.Manage;
using DriveOS.Modules.FundingBilling.Application.Refunds.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record RequestRefundRequest(decimal Amount, string Reason);
public sealed record CompleteRefundRequest(string? ProviderReference);
public sealed record RefundReasonRequest(string Reason);

public static class RefundEndpoints
{
    public static IEndpointRouteBuilder MapRefundEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/finance/refunds").WithTags("Funding & Billing - Refunds");
        g.MapPost("/payment/{paymentId:guid}",RequestAsync).Produces<Guid>(201).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsRequest);
        g.MapGet("/{refundId:guid}",GetAsync).Produces<RefundResponse>().RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsRead);
        g.MapGet("/by-payment/{paymentId:guid}",ListAsync).Produces<IReadOnlyCollection<RefundResponse>>().RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsRead);
        g.MapPost("/{refundId:guid}/approve",ApproveAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsApprove);
        g.MapPost("/{refundId:guid}/processing",ProcessingAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsProcess);
        g.MapPost("/{refundId:guid}/complete",CompleteAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsProcess);
        g.MapPost("/{refundId:guid}/reject",RejectAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsApprove);
        g.MapPost("/{refundId:guid}/fail",FailAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsProcess);
        g.MapPost("/{refundId:guid}/cancel",CancelAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.RefundsRequest);
        return endpoints;
    }
    static async Task<IResult> RequestAsync(Guid paymentId,RequestRefundRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new RequestRefundCommand(o,new PaymentId(paymentId),r.Amount,r.Reason,a),ct);return x.IsSuccess?Results.Created($"/api/finance/refunds/{x.Value.Value}",x.Value.Value):Problem(x.Error);}
    static async Task<IResult> GetAsync(Guid refundId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var x=await m.Send(new GetRefundQuery(t.OrganizationId.Value,new RefundId(refundId)),ct);return x.IsSuccess?Results.Ok(x.Value):Problem(x.Error);}
    static async Task<IResult> ListAsync(Guid paymentId,IMediator m,ICurrentTenant t,CancellationToken ct){if(!t.HasTenant||t.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var x=await m.Send(new GetPaymentRefundsQuery(t.OrganizationId.Value,new PaymentId(paymentId)),ct);return x.IsSuccess?Results.Ok(x.Value):Problem(x.Error);}
    static async Task<IResult> ApproveAsync(Guid refundId,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new ApproveRefundCommand(o,new RefundId(refundId),a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> ProcessingAsync(Guid refundId,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new MarkRefundProcessingCommand(o,new RefundId(refundId),a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> CompleteAsync(Guid refundId,CompleteRefundRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new CompleteRefundCommand(o,new RefundId(refundId),r.ProviderReference,a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> RejectAsync(Guid refundId,RefundReasonRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new RejectRefundCommand(o,new RefundId(refundId),r.Reason,a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> FailAsync(Guid refundId,RefundReasonRequest r,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new FailRefundCommand(o,new RefundId(refundId),r.Reason,a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static async Task<IResult> CancelAsync(Guid refundId,IMediator m,ICurrentTenant t,ICurrentUser u,CancellationToken ct){if(!Ctx(t,u,out var o,out var a,out var e))return e!;var x=await m.Send(new CancelRefundCommand(o,new RefundId(refundId),a),ct);return x.IsSuccess?Results.NoContent():Problem(x.Error);}
    static bool Ctx(ICurrentTenant t,ICurrentUser u,out OrganizationId o,out UserId a,out IResult? e){o=default;a=default;e=null;if(!t.HasTenant||t.OrganizationId is null){e=Results.Problem(statusCode:401,title:"errors.currentTenant.required");return false;}if(u.UserId is null){e=Results.Problem(statusCode:401,title:"errors.currentUser.required");return false;}o=t.OrganizationId.Value;a=u.UserId.Value;return true;}
    static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},title:e.Code,detail:e.MessageKey,extensions:new Dictionary<string,object?>{{"code",e.Code}});
}
