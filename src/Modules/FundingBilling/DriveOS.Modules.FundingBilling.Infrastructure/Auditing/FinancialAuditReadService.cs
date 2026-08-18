using DriveOS.Modules.FundingBilling.Application.Auditing;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Auditing;

internal sealed class FinancialAuditReadService(FundingBillingDbContext dbContext) : IFinancialAuditReadService
{
    public async Task<IReadOnlyList<FinancialAuditEntryResponse>> ListAsync(
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        CancellationToken cancellationToken = default) =>
        await dbContext.Set<FinancialAuditEntry>()
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.EventId)
            .Select(x => new FinancialAuditEntryResponse(
                x.EventId,
                x.BillingAccountId.Value,
                x.AggregateType,
                x.AggregateId,
                x.Action,
                x.ActorUserId.HasValue ? x.ActorUserId.Value.Value : null,
                x.OccurredAtUtc,
                x.DetailsJson))
            .ToListAsync(cancellationToken);
}
