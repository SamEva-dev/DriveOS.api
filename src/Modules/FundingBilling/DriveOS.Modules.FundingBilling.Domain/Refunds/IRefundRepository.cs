using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Refunds;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(RefundId id, CancellationToken cancellationToken = default);
    Task<decimal> GetCompletedAmountForPaymentAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
    Task<decimal> GetReservedAmountForPaymentAsync(PaymentId paymentId, RefundId? excludingRefundId = null, CancellationToken cancellationToken = default);
    Task<Refund?> GetByProviderReferenceAsync(OrganizationId organizationId, string providerReference, CancellationToken cancellationToken = default);
    Task AddAsync(Refund refund, CancellationToken cancellationToken = default);
}
