using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingParties;

public interface IBillingPartyRepository
{
    Task<BillingParty?> GetByIdAsync(BillingPartyId id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveAsync(BillingAccountId billingAccountId, PersonId? personId, OrganizationId? organizationId, BillingPartyRole role, CancellationToken cancellationToken = default);
    Task<bool> IsAuthorizedAsync(BillingAccountId billingAccountId, PersonId? personId, OrganizationId? organizationId, BillingPartyRole requiredRole, decimal amount, DateOnly businessDate, CancellationToken cancellationToken = default);
    Task AddAsync(BillingParty billingParty, CancellationToken cancellationToken = default);
}
