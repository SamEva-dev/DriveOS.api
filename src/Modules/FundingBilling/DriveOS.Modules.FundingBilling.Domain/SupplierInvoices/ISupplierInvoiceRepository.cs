using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
public interface ISupplierInvoiceRepository
{
    Task<SupplierInvoice?> GetAsync(SupplierInvoiceId id,bool tracking,CancellationToken ct=default);
    Task<SupplierInvoice?> GetByExternalSourceAsync(SupplierInvoiceSourceType sourceType,Guid externalSourceId,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<SupplierInvoice>> ListOverdueCandidatesAsync(DateOnly today,bool tracking,CancellationToken ct=default);
    void Add(SupplierInvoice invoice);
}
