using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
public static class SupplierInvoiceErrors
{
    public static readonly Error NotFound=Error.NotFound("Finance.SupplierInvoices.NotFound","errors.finance.supplierInvoices.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("Finance.SupplierInvoices.InvalidIdentifier","errors.finance.supplierInvoices.invalidIdentifier");
    public static readonly Error InvalidAmounts=Error.Validation("Finance.SupplierInvoices.InvalidAmounts","errors.finance.supplierInvoices.invalidAmounts");
    public static readonly Error InvalidCurrency=Error.Validation("Finance.SupplierInvoices.InvalidCurrency","errors.finance.supplierInvoices.invalidCurrency");
    public static readonly Error DuplicateSource=Error.Conflict("Finance.SupplierInvoices.DuplicateSource","errors.finance.supplierInvoices.duplicateSource");
    public static readonly Error InvalidTransition=Error.Conflict("Finance.SupplierInvoices.InvalidTransition","errors.finance.supplierInvoices.invalidTransition");
    public static readonly Error ReasonRequired=Error.Validation("Finance.SupplierInvoices.ReasonRequired","errors.finance.supplierInvoices.reasonRequired");
    public static readonly Error PaymentNotAllowed=Error.Conflict("Finance.SupplierInvoices.PaymentNotAllowed","errors.finance.supplierInvoices.paymentNotAllowed");
    public static readonly Error InvalidSettlementAmount=Error.Validation("Finance.SupplierInvoices.InvalidSettlementAmount","errors.finance.supplierInvoices.invalidSettlementAmount");
    public static readonly Error InvalidRefundAmount=Error.Validation("Finance.SupplierInvoices.InvalidRefundAmount","errors.finance.supplierInvoices.invalidRefundAmount");
}
