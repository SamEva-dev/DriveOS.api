using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;

internal sealed class InvoiceRepository(FundingBillingDbContext dbContext) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken cancellationToken = default) =>
        dbContext.Invoices.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.AddAsync(invoice, cancellationToken);
    public async Task<IReadOnlyCollection<Invoice>> ListDueAsync(OrganizationId organizationId, DateOnly beforeDate, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices.Include(x => x.Lines)
            .Where(x => x.OrganizationId == organizationId && x.DueDate < beforeDate && (x.Status == InvoiceStatus.Issued || x.Status == InvoiceStatus.PartiallyPaid))
            .ToListAsync(cancellationToken);
}

