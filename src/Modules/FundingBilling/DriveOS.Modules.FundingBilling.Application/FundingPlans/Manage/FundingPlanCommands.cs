using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.FundingPlans.Manage;

public sealed record SubmitFundingPlanCommand(OrganizationId OrganizationId, FundingPlanId FundingPlanId, UserId ActorUserId) : ICommand;
public sealed record ApproveFundingAllocationCommand(OrganizationId OrganizationId, FundingPlanId FundingPlanId, FundingAllocationId AllocationId, decimal ApprovedAmount, UserId ActorUserId) : ICommand;
public sealed record RejectFundingAllocationCommand(OrganizationId OrganizationId, FundingPlanId FundingPlanId, FundingAllocationId AllocationId, string Reason, UserId ActorUserId) : ICommand;

internal sealed class ApproveFundingAllocationCommandValidator : AbstractValidator<ApproveFundingAllocationCommand> { public ApproveFundingAllocationCommandValidator() => RuleFor(x => x.ApprovedAmount).GreaterThan(0m); }
internal sealed class RejectFundingAllocationCommandValidator : AbstractValidator<RejectFundingAllocationCommand> { public RejectFundingAllocationCommandValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000); }

internal abstract class FundingPlanHandlerBase(IFundingPlanRepository plans, IFundingBillingUnitOfWork unitOfWork, IClock clock)
{
    protected async Task<Result> Execute(OrganizationId organizationId, FundingPlanId planId, UserId actorUserId, Func<FundingPlan, DateTimeOffset, Result> action, CancellationToken ct, Func<FundingPlan, CancellationToken, Task>? afterCommit = null)
    {
        FundingPlan? plan = await plans.GetByIdAsync(planId, ct); if (plan is null || plan.OrganizationId != organizationId) return Result.Failure(FundingPlanErrors.NotFound);
        DateTimeOffset now = clock.UtcNow; Result result = action(plan, now); if (result.IsFailure) return result; plan.SetModifiedAudit(now, actorUserId); await unitOfWork.CommitAsync(ct);
        if (afterCommit is not null) await afterCommit(plan, ct);
        return Result.Success();
    }
}

internal sealed class SubmitFundingPlanCommandHandler(IFundingPlanRepository plans, IFundingBillingUnitOfWork uow, IClock clock) : FundingPlanHandlerBase(plans,uow,clock), ICommandHandler<SubmitFundingPlanCommand>
{ public Task<Result> Handle(SubmitFundingPlanCommand c, CancellationToken ct) => Execute(c.OrganizationId,c.FundingPlanId,c.ActorUserId,(p,now)=>p.Submit(c.ActorUserId,now),ct); }
internal sealed class ApproveFundingAllocationCommandHandler(IFundingPlanRepository plans, IFundingBillingUnitOfWork uow, IFinancialNotificationGateway notifications, IClock clock) : FundingPlanHandlerBase(plans,uow,clock), ICommandHandler<ApproveFundingAllocationCommand>
{ public Task<Result> Handle(ApproveFundingAllocationCommand c, CancellationToken ct) => Execute(c.OrganizationId,c.FundingPlanId,c.ActorUserId,(p,now)=>p.ApproveAllocation(c.AllocationId,c.ApprovedAmount,c.ActorUserId,now),ct,(p,token)=>notifications.QueueFundingDecisionAsync(c.OrganizationId,p.BillingAccountId,p.Status.ToString(),p.ApprovedFundingAmount,p.TotalCost,p.Currency,token)); }
internal sealed class RejectFundingAllocationCommandHandler(IFundingPlanRepository plans, IFundingBillingUnitOfWork uow, IFinancialNotificationGateway notifications, IClock clock) : FundingPlanHandlerBase(plans,uow,clock), ICommandHandler<RejectFundingAllocationCommand>
{ public Task<Result> Handle(RejectFundingAllocationCommand c, CancellationToken ct) => Execute(c.OrganizationId,c.FundingPlanId,c.ActorUserId,(p,now)=>p.RejectAllocation(c.AllocationId,c.Reason,c.ActorUserId,now),ct,(p,token)=>notifications.QueueFundingDecisionAsync(c.OrganizationId,p.BillingAccountId,p.Status.ToString(),p.ApprovedFundingAmount,p.TotalCost,p.Currency,token)); }
