using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class TrainingCreditAccountRepository(FundingBillingDbContext dbContext) : ITrainingCreditAccountRepository
{
    public Task<TrainingCreditAccount?> GetByIdAsync(TrainingCreditAccountId id, CancellationToken cancellationToken = default) =>
        dbContext.TrainingCreditAccounts.Include(x => x.Movements).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(BillingAccountId billingAccountId, string creditType, DateOnly? expirationDate, CancellationToken cancellationToken = default) =>
        dbContext.TrainingCreditAccounts.AsNoTracking().AnyAsync(
            x => x.BillingAccountId == billingAccountId && x.CreditType == creditType && x.ExpirationDate == expirationDate,
            cancellationToken);

    public Task<bool> MovementReferenceExistsAsync(TrainingCreditAccountId accountId, string reference, CancellationToken cancellationToken = default) =>
        dbContext.TrainingCreditMovements.AsNoTracking().AnyAsync(x => x.TrainingCreditAccountId == accountId && x.Reference == reference, cancellationToken);

    public Task<TrainingCreditMovement?> GetMovementByReferenceAsync(TrainingCreditAccountId accountId, string reference, CancellationToken cancellationToken = default) =>
        dbContext.TrainingCreditMovements.AsNoTracking().SingleOrDefaultAsync(
            x => x.TrainingCreditAccountId == accountId && x.Reference == reference, cancellationToken);

    public Task AddAsync(TrainingCreditAccount account, CancellationToken cancellationToken = default) =>
        dbContext.TrainingCreditAccounts.AddAsync(account, cancellationToken).AsTask();
}
