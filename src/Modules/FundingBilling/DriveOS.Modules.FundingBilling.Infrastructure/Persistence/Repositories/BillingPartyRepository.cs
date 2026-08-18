using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class BillingPartyRepository(FundingBillingDbContext dbContext) : IBillingPartyRepository
{
    public Task<BillingParty?> GetByIdAsync(BillingPartyId id, CancellationToken cancellationToken = default) => dbContext.BillingParties.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task AddAsync(BillingParty billingParty, CancellationToken cancellationToken = default) => dbContext.BillingParties.AddAsync(billingParty, cancellationToken).AsTask();
    public Task<bool> HasActiveAsync(BillingAccountId billingAccountId, PersonId? personId, OrganizationId? organizationId, BillingPartyRole role, CancellationToken cancellationToken = default) => dbContext.BillingParties.AsNoTracking().AnyAsync(x => x.BillingAccountId == billingAccountId && x.Status == BillingPartyStatus.Active && (x.Role == role || x.Role == BillingPartyRole.PayerAndFunder || role == BillingPartyRole.PayerAndFunder && (x.Role == BillingPartyRole.Payer || x.Role == BillingPartyRole.Funder)) && x.PersonId == personId && x.PartyOrganizationId == organizationId, cancellationToken);
    public Task<bool> IsAuthorizedAsync(BillingAccountId billingAccountId, PersonId? personId, OrganizationId? organizationId, BillingPartyRole requiredRole, decimal amount, DateOnly businessDate, CancellationToken cancellationToken = default)
    {
        return dbContext.BillingParties.AsNoTracking().AnyAsync(x => x.BillingAccountId == billingAccountId && x.Status == BillingPartyStatus.Active && x.PersonId == personId && x.PartyOrganizationId == organizationId && x.EffectiveFrom <= businessDate && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= businessDate) && (x.Role == requiredRole || x.Role == BillingPartyRole.PayerAndFunder) && (!x.MaximumAmount.HasValue || x.MaximumAmount.Value >= amount), cancellationToken);
    }
}
