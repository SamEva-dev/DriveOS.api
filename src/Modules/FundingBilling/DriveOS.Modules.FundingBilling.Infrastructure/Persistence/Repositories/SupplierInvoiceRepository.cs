using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class SupplierInvoiceRepository(FundingBillingDbContext db):ISupplierInvoiceRepository
{
    public Task<SupplierInvoice?> GetAsync(SupplierInvoiceId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.SupplierInvoices.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.SupplierInvoices.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<SupplierInvoice?> GetByExternalSourceAsync(
        SupplierInvoiceSourceType sourceType,Guid externalSourceId,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.SupplierInvoices.SingleOrDefaultAsync(x=>x.SourceType==sourceType&&x.ExternalSourceId==externalSourceId,ct)
            :db.SupplierInvoices.AsNoTracking().SingleOrDefaultAsync(x=>x.SourceType==sourceType&&x.ExternalSourceId==externalSourceId,ct);

    public async Task<IReadOnlyList<SupplierInvoice>> ListOverdueCandidatesAsync(
        DateOnly today,bool tracking,CancellationToken ct=default)
    {
        IQueryable<SupplierInvoice> q=db.SupplierInvoices.Where(x=>
            x.DueDate<today&&
            x.Status!=SupplierInvoiceStatus.Paid&&
            x.Status!=SupplierInvoiceStatus.Rejected&&
            x.Status!=SupplierInvoiceStatus.Disputed&&
            x.SettlementStatus!=SupplierInvoiceSettlementStatus.Refunded);

        if(!tracking)q=q.AsNoTracking();
        return await q.OrderBy(x=>x.DueDate).ToListAsync(ct);
    }

    public void Add(SupplierInvoice invoice)=>db.SupplierInvoices.Add(invoice);
}
