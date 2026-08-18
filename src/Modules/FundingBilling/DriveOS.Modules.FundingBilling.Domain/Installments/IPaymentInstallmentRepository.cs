using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Installments;

public interface IPaymentInstallmentRepository
{
    Task<PaymentInstallment?> GetByIdAsync(PaymentInstallmentId id, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentInstallment installment, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PaymentInstallment> installments, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PaymentInstallment>> ListDueAsync(OrganizationId organizationId, DateOnly beforeDate, CancellationToken cancellationToken = default);
}

