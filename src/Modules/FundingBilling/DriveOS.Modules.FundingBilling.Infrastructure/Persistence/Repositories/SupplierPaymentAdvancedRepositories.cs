using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class SupplierPaymentBatchRepository(
    FundingBillingDbContext db):ISupplierPaymentBatchRepository
{
    public void Add(SupplierPaymentBatch batch)=>db.SupplierPaymentBatches.Add(batch);
}

internal sealed class SupplierPaymentRefundRepository(
    FundingBillingDbContext db):ISupplierPaymentRefundRepository
{
    public async Task<IReadOnlyList<SupplierPaymentRefund>> ListByInvoiceAsync(
        SupplierInvoiceId invoiceId,CancellationToken ct=default)=>
        await db.SupplierPaymentRefunds.AsNoTracking()
            .Where(x=>x.SupplierInvoiceId==invoiceId)
            .OrderByDescending(x=>x.RefundedAtUtc)
            .ToListAsync(ct);

    public void Add(SupplierPaymentRefund refund)=>db.SupplierPaymentRefunds.Add(refund);
}
