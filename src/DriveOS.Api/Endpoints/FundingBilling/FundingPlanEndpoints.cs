using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.FundingBilling.Application.FundingPlans.Create;
using DriveOS.Modules.FundingBilling.Application.FundingPlans.Manage;
using DriveOS.Modules.FundingBilling.Application.FundingPlans.Read;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.FundingBilling;

public sealed record CreateFundingAllocationRequest(Guid? FinancingPersonId, Guid? FinancingOrganizationId, decimal RequestedAmount, string? ExternalReference);
public sealed record CreateFundingPlanRequest(Guid ContractId, decimal TotalCost, decimal StudentContribution, IReadOnlyCollection<CreateFundingAllocationRequest> Allocations);
public sealed record ApproveFundingAllocationRequest(decimal ApprovedAmount);
public sealed record RejectFundingAllocationRequest(string Reason);

public static class FundingPlanEndpoints
{
    public static IEndpointRouteBuilder MapFundingPlanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group=endpoints.MapGroup("/api/finance").WithTags("Funding & Billing - Funding plans");
        group.MapPost("/billing-accounts/{billingAccountId:guid}/funding-plans",CreateAsync).Produces<Guid>(201).RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansManage);
        group.MapGet("/billing-accounts/{billingAccountId:guid}/funding-plans",ListAsync).Produces<IReadOnlyCollection<FundingPlanResponse>>().RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansRead);
        group.MapGet("/funding-plans/{fundingPlanId:guid}",GetAsync).Produces<FundingPlanResponse>().RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansRead);
        group.MapPost("/funding-plans/{fundingPlanId:guid}/submit",SubmitAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansManage);
        group.MapPost("/funding-plans/{fundingPlanId:guid}/allocations/{allocationId:guid}/approve",ApproveAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansApprove);
        group.MapPost("/funding-plans/{fundingPlanId:guid}/allocations/{allocationId:guid}/reject",RejectAsync).Produces(204).RequireAuthorization(DriveOsPermissionCodes.Finance.FundingPlansApprove);
        return endpoints;
    }
    private static async Task<IResult> CreateAsync(Guid billingAccountId,CreateFundingPlanRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(!TryContext(tenant,user,out var org,out var actor,out var error))return error!;var allocations=request.Allocations.Select(x=>new CreateFundingAllocationInput(x.FinancingPersonId,x.FinancingOrganizationId,x.RequestedAmount,x.ExternalReference)).ToArray();var result=await mediator.Send(new CreateFundingPlanCommand(org,new BillingAccountId(billingAccountId),request.ContractId,request.TotalCost,request.StudentContribution,allocations,actor),ct);return result.IsSuccess?Results.Created($"/api/finance/funding-plans/{result.Value.Value}",result.Value.Value):Problem(result.Error);}
    private static async Task<IResult> ListAsync(Guid billingAccountId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct){if(!tenant.HasTenant||tenant.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var r=await mediator.Send(new GetBillingAccountFundingPlansQuery(tenant.OrganizationId.Value,new BillingAccountId(billingAccountId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
    private static async Task<IResult> GetAsync(Guid fundingPlanId,IMediator mediator,ICurrentTenant tenant,CancellationToken ct){if(!tenant.HasTenant||tenant.OrganizationId is null)return Results.Problem(statusCode:401,title:"errors.currentTenant.required");var r=await mediator.Send(new GetFundingPlanQuery(tenant.OrganizationId.Value,new FundingPlanId(fundingPlanId)),ct);return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);}
    private static async Task<IResult> SubmitAsync(Guid fundingPlanId,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(!TryContext(tenant,user,out var org,out var actor,out var error))return error!;var r=await mediator.Send(new SubmitFundingPlanCommand(org,new FundingPlanId(fundingPlanId),actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> ApproveAsync(Guid fundingPlanId,Guid allocationId,ApproveFundingAllocationRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(!TryContext(tenant,user,out var org,out var actor,out var error))return error!;var r=await mediator.Send(new ApproveFundingAllocationCommand(org,new FundingPlanId(fundingPlanId),new FundingAllocationId(allocationId),request.ApprovedAmount,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> RejectAsync(Guid fundingPlanId,Guid allocationId,RejectFundingAllocationRequest request,IMediator mediator,ICurrentTenant tenant,ICurrentUser user,CancellationToken ct){if(!TryContext(tenant,user,out var org,out var actor,out var error))return error!;var r=await mediator.Send(new RejectFundingAllocationCommand(org,new FundingPlanId(fundingPlanId),new FundingAllocationId(allocationId),request.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static bool TryContext(ICurrentTenant tenant,ICurrentUser user,out OrganizationId organizationId,out UserId userId,out IResult? error){organizationId=default;userId=default;error=null;if(!tenant.HasTenant||tenant.OrganizationId is null){error=Results.Problem(statusCode:401,title:"errors.currentTenant.required");return false;}if(user.UserId is null){error=Results.Problem(statusCode:401,title:"errors.currentUser.required");return false;}organizationId=tenant.OrganizationId.Value;userId=user.UserId.Value;return true;}
    private static IResult Problem(Error e)=>Results.Problem(statusCode:e.Type switch{ErrorType.NotFound=>404,ErrorType.Conflict=>409,ErrorType.Validation=>400,_=>400},title:e.Code,detail:e.MessageKey,extensions:new Dictionary<string,object?>{{"code",e.Code}});
}
