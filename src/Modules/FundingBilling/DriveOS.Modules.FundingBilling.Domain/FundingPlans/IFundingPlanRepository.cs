using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans;

public interface IFundingPlanRepository
{
    Task<FundingPlan?> GetByIdAsync(FundingPlanId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForContractAsync(OrganizationId organizationId, Guid contractId, CancellationToken cancellationToken = default);
    Task AddAsync(FundingPlan fundingPlan, CancellationToken cancellationToken = default);
}
