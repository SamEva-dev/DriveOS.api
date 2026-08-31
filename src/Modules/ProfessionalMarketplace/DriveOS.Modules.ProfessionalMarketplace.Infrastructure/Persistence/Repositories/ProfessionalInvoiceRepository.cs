using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalInvoiceRepository(ProfessionalMarketplaceDbContext db):IProfessionalInvoiceRepository
{
    public Task<ProfessionalInvoice?> GetAsync(ProfessionalInvoiceId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ProfessionalInvoices.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalInvoices.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> ExistsForStatementAsync(ServiceStatementId statementId,CancellationToken ct=default)=>
        db.ProfessionalInvoices.AsNoTracking().AnyAsync(x=>x.ServiceStatementId==statementId&&x.Status!=ProfessionalInvoiceStatus.Cancelled,ct);
    public async Task<IReadOnlyList<ProfessionalInvoice>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        await db.ProfessionalInvoices.AsNoTracking().Where(x=>x.EngagementId==engagementId).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public Task<ProfessionalInvoice?> GetEarliestPaidAsync(
        ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        db.ProfessionalInvoices.AsNoTracking()
            .Where(x=>x.EngagementId==engagementId&&x.PaymentStatus==ProfessionalInvoicePaymentStatus.Paid)
            .OrderBy(x=>x.FinanceStatusSyncedAtUtc)
            .ThenBy(x=>x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public void Add(ProfessionalInvoice invoice)=>db.ProfessionalInvoices.Add(invoice);
}
