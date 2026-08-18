using DriveOS.Modules.FundingBilling.Application.TrainingCredits.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class TrainingCreditAccountReadService(FundingBillingDbContext dbContext) : ITrainingCreditAccountReadService
{
    public async Task<IReadOnlyCollection<TrainingCreditAccountResponse>> ListAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.TrainingCreditAccounts.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .OrderBy(x => x.CreditType)
            .ThenBy(x => x.ExpirationDate)
            .Select(x => new
            {
                x.Id,
                x.BillingAccountId,
                x.CreditType,
                x.QuantityPurchased,
                x.QuantityReserved,
                x.QuantityConsumed,
                x.Adjustments,
                x.ExpirationDate,
                x.Status
            })
            .ToArrayAsync(cancellationToken);

        return rows.Select(x => new TrainingCreditAccountResponse(
            x.Id.Value,
            x.BillingAccountId.Value,
            x.CreditType,
            x.QuantityPurchased,
            x.QuantityReserved,
            x.QuantityConsumed,
            x.Adjustments,
            decimal.Round(x.QuantityPurchased - x.QuantityReserved - x.QuantityConsumed + x.Adjustments, 2, MidpointRounding.AwayFromZero),
            x.ExpirationDate,
            x.Status.ToString())).ToArray();
    }

    public async Task<TrainingCreditAccountResponse?> GetAsync(OrganizationId organizationId, TrainingCreditAccountId id, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.TrainingCreditAccounts.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.BillingAccountId,
                x.CreditType,
                x.QuantityPurchased,
                x.QuantityReserved,
                x.QuantityConsumed,
                x.Adjustments,
                x.ExpirationDate,
                x.Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : new TrainingCreditAccountResponse(
            row.Id.Value,
            row.BillingAccountId.Value,
            row.CreditType,
            row.QuantityPurchased,
            row.QuantityReserved,
            row.QuantityConsumed,
            row.Adjustments,
            decimal.Round(row.QuantityPurchased - row.QuantityReserved - row.QuantityConsumed + row.Adjustments, 2, MidpointRounding.AwayFromZero),
            row.ExpirationDate,
            row.Status.ToString());
    }


    public async Task<IReadOnlyCollection<TrainingCreditMovementResponse>> ListMovementsAsync(OrganizationId organizationId, TrainingCreditAccountId id, CancellationToken cancellationToken = default)
    {
        bool exists = await dbContext.TrainingCreditAccounts.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);
        if (!exists) return Array.Empty<TrainingCreditMovementResponse>();

        var rows = await dbContext.TrainingCreditMovements.AsNoTracking()
            .Where(x => x.TrainingCreditAccountId == id)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new { x.Id, x.Type, x.Quantity, x.Reference, x.Reason, x.OccurredAtUtc, x.ActorUserId })
            .ToArrayAsync(cancellationToken);

        return rows.Select(x => new TrainingCreditMovementResponse(
            x.Id.Value, x.Type.ToString(), x.Quantity, x.Reference, x.Reason, x.OccurredAtUtc, x.ActorUserId.Value)).ToArray();
    }
}
