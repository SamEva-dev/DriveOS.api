using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

public interface ISupplierPaymentBatchRepository
{
    void Add(SupplierPaymentBatch batch);
}

public interface ISupplierPaymentRefundRepository
{
    Task<IReadOnlyList<SupplierPaymentRefund>> ListByInvoiceAsync(
        SupplierInvoiceId invoiceId,CancellationToken ct=default);
    void Add(SupplierPaymentRefund refund);
}
