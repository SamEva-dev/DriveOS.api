using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class FundingPlanRepository(FundingBillingDbContext dbContext) : IFundingPlanRepository
{
    public Task<FundingPlan?> GetByIdAsync(FundingPlanId id, CancellationToken cancellationToken = default) => dbContext.FundingPlans.Include(x=>x.Allocations).SingleOrDefaultAsync(x=>x.Id==id,cancellationToken);
    public Task<bool> ExistsForContractAsync(OrganizationId organizationId, Guid contractId, CancellationToken cancellationToken = default) => dbContext.FundingPlans.AsNoTracking().AnyAsync(x=>x.OrganizationId==organizationId&&x.ContractId==contractId,cancellationToken);
    public Task AddAsync(FundingPlan fundingPlan, CancellationToken cancellationToken = default) => dbContext.FundingPlans.AddAsync(fundingPlan,cancellationToken).AsTask();
}
