using DriveOS.Modules.FundingBilling.Application.CreditNotes.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class CreditNoteReadService(FundingBillingDbContext db) : ICreditNoteReadService
{
    public async Task<CreditNoteResponse?> GetAsync(OrganizationId organizationId, CreditNoteId id, CancellationToken cancellationToken = default)
    {
        var row = await db.CreditNotes.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == id)
            .Select(x => new
            {
                x.Id, x.InvoiceId, x.BillingAccountId, x.Currency, x.Reason, x.CreditNoteNumber,
                x.IssueDate, x.Status, x.CreatedAtUtc, x.IssuedAtUtc,
                Lines = x.Lines.Select(l => new CreditNoteLineResponse(
                    l.Id.Value,
                    l.InvoiceLineId.HasValue ? l.InvoiceLineId.Value.Value : null,
                    l.Description, l.Quantity, l.Unit, l.UnitPrice, l.DiscountAmount, l.TaxRate,
                    l.NetAmount, l.TaxAmount, l.TotalAmount)).ToArray()
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null) return null;
        return new CreditNoteResponse(row.Id.Value, row.InvoiceId.Value, row.BillingAccountId.Value, row.Currency,
            row.Reason, row.CreditNoteNumber, row.IssueDate, row.Status.ToString(),
            row.Lines.Sum(x => x.NetAmount), row.Lines.Sum(x => x.TaxAmount), row.Lines.Sum(x => x.TotalAmount),
            row.CreatedAtUtc, row.IssuedAtUtc, row.Lines);
    }

    public async Task<IReadOnlyCollection<CreditNoteResponse>> ListByInvoiceAsync(OrganizationId organizationId, InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        var rows = await db.CreditNotes.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.InvoiceId == invoiceId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id, x.InvoiceId, x.BillingAccountId, x.Currency, x.Reason, x.CreditNoteNumber,
                x.IssueDate, x.Status, x.CreatedAtUtc, x.IssuedAtUtc,
                Lines = x.Lines.Select(l => new CreditNoteLineResponse(
                    l.Id.Value,
                    l.InvoiceLineId.HasValue ? l.InvoiceLineId.Value.Value : null,
                    l.Description, l.Quantity, l.Unit, l.UnitPrice, l.DiscountAmount, l.TaxRate,
                    l.NetAmount, l.TaxAmount, l.TotalAmount)).ToArray()
            }).ToListAsync(cancellationToken);

        return rows.Select(row => new CreditNoteResponse(row.Id.Value, row.InvoiceId.Value, row.BillingAccountId.Value,
            row.Currency, row.Reason, row.CreditNoteNumber, row.IssueDate, row.Status.ToString(),
            row.Lines.Sum(x => x.NetAmount), row.Lines.Sum(x => x.TaxAmount), row.Lines.Sum(x => x.TotalAmount),
            row.CreatedAtUtc, row.IssuedAtUtc, row.Lines)).ToArray();
    }
}
