using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.FundingBilling.FundingPlans;

public sealed class FundingPlanTests
{
    [Fact]
    public void AddAllocation_ShouldRejectCoverageAboveTotalCost()
    {
        var plan=CreatePlan(1000m,200m);
        var result=plan.AddAllocation(FundingAllocationId.New(),new PersonId(Guid.NewGuid()),null,900m,null);
        Assert.True(result.IsFailure); Assert.Equal(FundingPlanErrors.AllocationExceeded.Code,result.Error.Code);
    }

    [Fact]
    public void Submit_ShouldRequireFullPlannedCoverage()
    {
        var plan=CreatePlan(1000m,200m); plan.AddAllocation(FundingAllocationId.New(),new PersonId(Guid.NewGuid()),null,500m,null);
        var result=plan.Submit(new UserId(Guid.NewGuid()),DateTimeOffset.UtcNow);
        Assert.True(result.IsFailure); Assert.Equal(FundingPlanErrors.CoverageIncomplete.Code,result.Error.Code);
    }

    [Fact]
    public void ApprovingAllAllocations_ShouldApprovePlan()
    {
        var plan=CreatePlan(1000m,200m); var allocation=plan.AddAllocation(FundingAllocationId.New(),new PersonId(Guid.NewGuid()),null,800m,"FUNDER-001"); Assert.True(allocation.IsSuccess); Assert.True(plan.Submit(new UserId(Guid.NewGuid()),DateTimeOffset.UtcNow).IsSuccess);
        var result=plan.ApproveAllocation(allocation.Value,800m,new UserId(Guid.NewGuid()),DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess); Assert.Equal(FundingPlanStatus.Approved,plan.Status); Assert.Equal(0m,plan.RemainingToApprove);
    }

    private static FundingPlan CreatePlan(decimal total,decimal contribution)=>FundingPlan.Create(FundingPlanId.New(),new OrganizationId(Guid.NewGuid()),new BillingAccountId(Guid.NewGuid()),new PersonId(Guid.NewGuid()),Guid.NewGuid(),total,contribution,"EUR").Value;
}
