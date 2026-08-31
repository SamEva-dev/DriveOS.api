using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

public interface ISupplierPaymentAttemptRepository
{
    Task<SupplierPaymentAttempt?> GetAsync(SupplierPaymentAttemptId id,bool tracking,CancellationToken ct=default);
    Task<bool> HasActiveAttemptAsync(SupplierInvoiceId supplierInvoiceId,CancellationToken ct=default);
    Task<IReadOnlyList<SupplierPaymentAttempt>> ListByInvoiceAsync(SupplierInvoiceId supplierInvoiceId,CancellationToken ct=default);
    void Add(SupplierPaymentAttempt attempt);
}
