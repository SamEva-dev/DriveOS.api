using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class SupplierPaymentAttemptRepository(FundingBillingDbContext db):ISupplierPaymentAttemptRepository
{
    public Task<SupplierPaymentAttempt?> GetAsync(SupplierPaymentAttemptId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.SupplierPaymentAttempts.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.SupplierPaymentAttempts.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> HasActiveAttemptAsync(SupplierInvoiceId supplierInvoiceId,CancellationToken ct=default)=>
        db.SupplierPaymentAttempts.AsNoTracking().AnyAsync(x=>
            x.SupplierInvoiceId==supplierInvoiceId&&
            (x.Status==SupplierPaymentAttemptStatus.Scheduled||x.Status==SupplierPaymentAttemptStatus.Processing),ct);

    public async Task<IReadOnlyList<SupplierPaymentAttempt>> ListByInvoiceAsync(SupplierInvoiceId supplierInvoiceId,CancellationToken ct=default)=>
        await db.SupplierPaymentAttempts.AsNoTracking()
            .Where(x=>x.SupplierInvoiceId==supplierInvoiceId)
            .OrderBy(x=>x.CreatedAtUtc)
            .ToListAsync(ct);

    public void Add(SupplierPaymentAttempt attempt)=>db.SupplierPaymentAttempts.Add(attempt);
}
