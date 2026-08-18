using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits;

public interface ITrainingCreditAccountRepository
{
    Task<TrainingCreditAccount?> GetByIdAsync(TrainingCreditAccountId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(BillingAccountId billingAccountId, string creditType, DateOnly? expirationDate, CancellationToken cancellationToken = default);
    Task<bool> MovementReferenceExistsAsync(TrainingCreditAccountId accountId, string reference, CancellationToken cancellationToken = default);
    Task AddAsync(TrainingCreditAccount account, CancellationToken cancellationToken = default);
}
