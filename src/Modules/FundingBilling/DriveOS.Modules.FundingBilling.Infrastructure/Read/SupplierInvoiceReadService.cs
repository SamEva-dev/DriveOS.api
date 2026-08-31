using DriveOS.Modules.FundingBilling.Application.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class SupplierInvoiceReadService(FundingBillingDbContext db):ISupplierInvoiceReadService
{
    public Task<SupplierInvoiceSnapshot?> GetByExternalSourceAsync(
        SupplierInvoiceSourceType sourceType,
        Guid externalSourceId,
        CancellationToken ct=default)=>
        db.SupplierInvoices.AsNoTracking()
            .Where(x=>x.SourceType==sourceType&&x.ExternalSourceId==externalSourceId)
            .Select(x=>new SupplierInvoiceSnapshot(
                x.Id.Value,x.ExternalSourceId,x.Status.ToString(),x.Subtotal+x.TaxAmount,x.Currency,x.DueDate))
            .SingleOrDefaultAsync(ct);
}
