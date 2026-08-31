using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.SupplierPayments;

public static class SupplierPaymentAttemptErrors
{
    public static readonly Error NotFound=Error.NotFound("Finance.SupplierPayments.NotFound","errors.finance.supplierPayments.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("Finance.SupplierPayments.InvalidIdentifier","errors.finance.supplierPayments.invalidIdentifier");
    public static readonly Error InvalidAmount=Error.Validation("Finance.SupplierPayments.InvalidAmount","errors.finance.supplierPayments.invalidAmount");
    public static readonly Error InvalidCurrency=Error.Validation("Finance.SupplierPayments.InvalidCurrency","errors.finance.supplierPayments.invalidCurrency");
    public static readonly Error InvalidPaymentMethod=Error.Validation("Finance.SupplierPayments.InvalidPaymentMethod","errors.finance.supplierPayments.invalidPaymentMethod");
    public static readonly Error InvalidTransition=Error.Conflict("Finance.SupplierPayments.InvalidTransition","errors.finance.supplierPayments.invalidTransition");
    public static readonly Error FailureReasonRequired=Error.Validation("Finance.SupplierPayments.FailureReasonRequired","errors.finance.supplierPayments.failureReasonRequired");
    public static readonly Error ActiveAttemptExists=Error.Conflict("Finance.SupplierPayments.ActiveAttemptExists","errors.finance.supplierPayments.activeAttemptExists");
    public static readonly Error InvoiceNotApproved=Error.Conflict("Finance.SupplierPayments.InvoiceNotApproved","errors.finance.supplierPayments.invoiceNotApproved");
    public static readonly Error MixedCurrenciesNotAllowed=Error.Validation("Finance.SupplierPayments.MixedCurrenciesNotAllowed","errors.finance.supplierPayments.mixedCurrenciesNotAllowed");
}
