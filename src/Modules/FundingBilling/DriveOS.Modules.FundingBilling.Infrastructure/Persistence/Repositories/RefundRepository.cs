using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class RefundRepository(FundingBillingDbContext dbContext) : IRefundRepository
{
    public Task<Refund?> GetByIdAsync(RefundId id, CancellationToken cancellationToken = default) => dbContext.Refunds.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<decimal> GetCompletedAmountForPaymentAsync(PaymentId paymentId, CancellationToken cancellationToken = default) =>
        await dbContext.Refunds.Where(x => x.PaymentId == paymentId && x.Status == RefundStatus.Completed).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
    public async Task<decimal> GetReservedAmountForPaymentAsync(PaymentId paymentId, RefundId? excludingRefundId = null, CancellationToken cancellationToken = default) =>
        await dbContext.Refunds.Where(x => x.PaymentId == paymentId && x.Status != RefundStatus.Rejected && x.Status != RefundStatus.Failed && x.Status != RefundStatus.Cancelled && (!excludingRefundId.HasValue || x.Id != excludingRefundId.Value)).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
    public Task<Refund?> GetByProviderReferenceAsync(OrganizationId organizationId, string providerReference, CancellationToken cancellationToken = default) => dbContext.Refunds.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ProviderReference == providerReference, cancellationToken);
    public Task AddAsync(Refund refund, CancellationToken cancellationToken = default) => dbContext.Refunds.AddAsync(refund, cancellationToken).AsTask();
}
