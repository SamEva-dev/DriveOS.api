using DriveOS.Modules.FundingBilling.Application.Invoices.Read;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Read;

internal sealed class InvoiceReadService(FundingBillingDbContext dbContext) : IInvoiceReadService
{
    public async Task<InvoiceResponse?> GetByIdAsync(OrganizationId organizationId, InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Id == invoiceId)
            .Select(x => new
            {
                x.Id, x.BillingAccountId, x.CustomerPersonId, x.Currency, x.InvoiceNumber,
                x.IssueDate, x.DueDate, x.Status, x.PaidAmount, x.CreditedAmount, x.IssuedAtUtc,
                Lines = x.Lines.OrderBy(l => l.Description).Select(l => new InvoiceLineResponse(
                    l.Id, l.Description, l.Quantity, l.Unit, l.UnitPrice, l.DiscountAmount,
                    l.TaxRate, l.NetAmount, l.TaxAmount, l.TotalAmount)).ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null) return null;
        return new InvoiceResponse(row.Id, row.BillingAccountId, row.CustomerPersonId, row.Currency,
            row.InvoiceNumber, row.IssueDate, row.DueDate, row.Status.ToString(),
            row.Lines.Sum(l => l.NetAmount), row.Lines.Sum(l => l.TaxAmount), row.Lines.Sum(l => l.TotalAmount),
            row.PaidAmount, row.CreditedAmount, decimal.Max(0m, row.Lines.Sum(l => l.TotalAmount) - row.CreditedAmount), decimal.Max(0m, row.Lines.Sum(l => l.TotalAmount) - row.PaidAmount - row.CreditedAmount), row.IssuedAtUtc, row.Lines);
    }

    public async Task<IReadOnlyCollection<InvoiceResponse>> ListByBillingAccountAsync(OrganizationId organizationId, BillingAccountId billingAccountId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.BillingAccountId == billingAccountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                x.Id, x.BillingAccountId, x.CustomerPersonId, x.Currency, x.InvoiceNumber,
                x.IssueDate, x.DueDate, x.Status, x.PaidAmount, x.CreditedAmount, x.IssuedAtUtc,
                Lines = x.Lines.Select(l => new InvoiceLineResponse(
                    l.Id, l.Description, l.Quantity, l.Unit, l.UnitPrice, l.DiscountAmount,
                    l.TaxRate, l.NetAmount, l.TaxAmount, l.TotalAmount)).ToArray()
            })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new InvoiceResponse(row.Id, row.BillingAccountId, row.CustomerPersonId, row.Currency,
            row.InvoiceNumber, row.IssueDate, row.DueDate, row.Status.ToString(),
            row.Lines.Sum(l => l.NetAmount), row.Lines.Sum(l => l.TaxAmount), row.Lines.Sum(l => l.TotalAmount),
            row.PaidAmount, row.CreditedAmount, decimal.Max(0m, row.Lines.Sum(l => l.TotalAmount) - row.CreditedAmount), decimal.Max(0m, row.Lines.Sum(l => l.TotalAmount) - row.PaidAmount - row.CreditedAmount), row.IssuedAtUtc, row.Lines)).ToArray();
    }
}
