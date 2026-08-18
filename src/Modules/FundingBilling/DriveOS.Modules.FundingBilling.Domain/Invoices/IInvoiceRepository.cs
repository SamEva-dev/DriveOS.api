using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(InvoiceId id, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Invoice>> ListDueAsync(OrganizationId organizationId, DateOnly beforeDate, CancellationToken cancellationToken = default);
}

