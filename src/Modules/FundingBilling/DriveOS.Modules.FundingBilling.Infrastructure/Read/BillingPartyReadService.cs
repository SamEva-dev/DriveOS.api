using DriveOS.Modules.FundingBilling.Application.BillingParties.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class BillingPartyReadService(FundingBillingDbContext dbContext) : IBillingPartyReadService
{
    public async Task<IReadOnlyCollection<BillingPartyResponse>> ListAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.BillingParties.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId).OrderBy(x => x.Priority).ThenByDescending(x => x.IsPrimary).Select(x => new { x.Id, x.BillingAccountId, x.PersonId, x.PartyOrganizationId, x.Role, x.MaximumAmount, x.EffectiveFrom, x.EffectiveTo, x.Priority, x.IsPrimary, x.Status, x.EndReason, x.EndedAtUtc }).ToArrayAsync(cancellationToken);
        return rows.Select(x => new BillingPartyResponse(x.Id.Value, x.BillingAccountId.Value, x.PersonId.HasValue ? x.PersonId.Value.Value : null, x.PartyOrganizationId.HasValue ? x.PartyOrganizationId.Value.Value : null, x.Role.ToString(), x.MaximumAmount, x.EffectiveFrom, x.EffectiveTo, x.Priority, x.IsPrimary, x.Status.ToString(), x.EndReason, x.EndedAtUtc)).ToArray();
    }
}
