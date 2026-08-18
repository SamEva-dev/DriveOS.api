using DriveOS.Modules.FundingBilling.Application.FundingPlans.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class FundingPlanReadService(FundingBillingDbContext dbContext) : IFundingPlanReadService
{
    public async Task<FundingPlanResponse?> GetAsync(OrganizationId organizationId, FundingPlanId fundingPlanId, CancellationToken cancellationToken = default)
    {
        var row=await dbContext.FundingPlans.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.Id==fundingPlanId).Select(x=>new{x.Id,x.BillingAccountId,x.StudentId,x.ContractId,x.TotalCost,x.StudentContribution,x.Currency,x.Status,x.SubmittedAtUtc,x.ApprovedAtUtc,Allocations=x.Allocations.OrderBy(a=>a.Id).Select(a=>new{a.Id,a.FinancingPersonId,a.FinancingOrganizationId,a.RequestedAmount,a.ApprovedAmount,a.ExternalReference,a.Status,a.DecidedAtUtc,a.DecidedByUserId,a.DecisionReason}).ToArray()}).SingleOrDefaultAsync(cancellationToken);
        if(row is null)return null; var allocations=row.Allocations.Select(a=>new FundingAllocationResponse(a.Id.Value,a.FinancingPersonId?.Value,a.FinancingOrganizationId?.Value,a.RequestedAmount,a.ApprovedAmount,a.ExternalReference,a.Status.ToString(),a.DecidedAtUtc,a.DecidedByUserId?.Value,a.DecisionReason)).ToArray(); decimal requested=decimal.Round(allocations.Sum(x=>x.RequestedAmount),2,MidpointRounding.AwayFromZero); decimal approved=decimal.Round(allocations.Where(x=>x.Status is "Approved" or "Exhausted").Sum(x=>x.ApprovedAmount),2,MidpointRounding.AwayFromZero);
        return new FundingPlanResponse(row.Id.Value,row.BillingAccountId.Value,row.StudentId.Value,row.ContractId,row.TotalCost,row.StudentContribution,requested,approved,decimal.Max(0m,row.TotalCost-row.StudentContribution-requested),decimal.Max(0m,row.TotalCost-row.StudentContribution-approved),row.Currency,row.Status.ToString(),row.SubmittedAtUtc,row.ApprovedAtUtc,allocations);
    }
    public async Task<IReadOnlyCollection<FundingPlanResponse>> ListByBillingAccountAsync(OrganizationId organizationId,BillingAccountId billingAccountId,CancellationToken cancellationToken=default)
    { var ids=await dbContext.FundingPlans.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.BillingAccountId==billingAccountId).OrderByDescending(x=>x.CreatedAtUtc).Select(x=>x.Id).ToArrayAsync(cancellationToken); var list=new List<FundingPlanResponse>(ids.Length);foreach(var id in ids){var value=await GetAsync(organizationId,id,cancellationToken);if(value is not null)list.Add(value);}return list; }
}
