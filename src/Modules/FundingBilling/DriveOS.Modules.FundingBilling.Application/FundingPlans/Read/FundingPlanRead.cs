using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.FundingPlans.Read;

public sealed record FundingAllocationResponse(Guid Id, Guid? FinancingPersonId, Guid? FinancingOrganizationId, decimal RequestedAmount, decimal ApprovedAmount, string? ExternalReference, string Status, DateTimeOffset? DecidedAtUtc, Guid? DecidedByUserId, string? DecisionReason);
public sealed record FundingPlanResponse(Guid Id, Guid BillingAccountId, Guid StudentId, Guid ContractId, decimal TotalCost, decimal StudentContribution, decimal RequestedFundingAmount, decimal ApprovedFundingAmount, decimal RemainingToPlan, decimal RemainingToApprove, string Currency, string Status, DateTimeOffset? SubmittedAtUtc, DateTimeOffset? ApprovedAtUtc, IReadOnlyCollection<FundingAllocationResponse> Allocations);
public interface IFundingPlanReadService
{
    Task<FundingPlanResponse?> GetAsync(OrganizationId organizationId, FundingPlanId fundingPlanId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FundingPlanResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default);
}
public sealed record GetFundingPlanQuery(OrganizationId OrganizationId, FundingPlanId FundingPlanId) : IQuery<FundingPlanResponse>;
public sealed record GetBillingAccountFundingPlansQuery(OrganizationId OrganizationId, BillingAccountId BillingAccountId) : IQuery<IReadOnlyCollection<FundingPlanResponse>>;
internal sealed class GetFundingPlanQueryHandler(IFundingPlanReadService readService) : IQueryHandler<GetFundingPlanQuery, FundingPlanResponse>
{ public async Task<Result<FundingPlanResponse>> Handle(GetFundingPlanQuery q,CancellationToken ct){var value=await readService.GetAsync(q.OrganizationId,q.FundingPlanId,ct);return value is null?Result.Failure<FundingPlanResponse>(FundingPlanErrors.NotFound):Result.Success(value);} }
internal sealed class GetBillingAccountFundingPlansQueryHandler(IFundingPlanReadService readService) : IQueryHandler<GetBillingAccountFundingPlansQuery,IReadOnlyCollection<FundingPlanResponse>>
{ public async Task<Result<IReadOnlyCollection<FundingPlanResponse>>> Handle(GetBillingAccountFundingPlansQuery q,CancellationToken ct)=>Result.Success(await readService.ListByBillingAccountAsync(q.OrganizationId,q.BillingAccountId,ct)); }
