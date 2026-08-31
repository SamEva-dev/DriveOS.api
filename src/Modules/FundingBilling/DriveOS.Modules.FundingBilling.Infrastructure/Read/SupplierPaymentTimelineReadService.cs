using DriveOS.Modules.FundingBilling.Application.SupplierPayments;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class SupplierPaymentTimelineReadService(FundingBillingDbContext db):ISupplierPaymentTimelineReadService
{
    public async Task<IReadOnlyList<SupplierPaymentAttemptSnapshot>> ListAsync(
        SupplierInvoiceId supplierInvoiceId,
        CancellationToken ct=default)=>
        await db.SupplierPaymentAttempts.AsNoTracking()
            .Where(x=>x.SupplierInvoiceId==supplierInvoiceId)
            .OrderBy(x=>x.CreatedAtUtc)
            .Select(x=>new SupplierPaymentAttemptSnapshot(
                x.Id.Value,x.Status.ToString(),x.Amount,x.SettledAmount,x.Currency,x.PaymentMethod,
                x.ScheduledDate,x.SettledOn,x.CreatedAtUtc,x.ProcessingAtUtc,x.PaidAtUtc,x.FailedAtUtc,
                x.CancelledAtUtc,x.ProviderReference,x.FailureReason,x.ReconciliationStatus.ToString(),
                x.ReconciliationDifference,x.BatchId==null?null:x.BatchId.Value.Value,x.IsManual))
            .ToListAsync(ct);
}
