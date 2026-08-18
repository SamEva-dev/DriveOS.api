using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts;

public static class BillingAccountErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "FundingBilling.BillingAccount.Id.Invalid",
        "errors.fundingBilling.billingAccount.id.invalid");

    public static readonly Error InvalidOwner = Error.Validation(
        "FundingBilling.BillingAccount.Owner.Invalid",
        "errors.fundingBilling.billingAccount.owner.invalid");

    public static readonly Error InvalidCurrency = Error.Validation(
        "FundingBilling.BillingAccount.Currency.Invalid",
        "errors.fundingBilling.billingAccount.currency.invalid");

    public static readonly Error InvalidReason = Error.Validation(
        "FundingBilling.BillingAccount.Reason.Invalid",
        "errors.fundingBilling.billingAccount.reason.invalid");

    public static readonly Error NotFound = Error.NotFound(
        "FundingBilling.BillingAccount.NotFound",
        "errors.fundingBilling.billingAccount.notFound");

    public static readonly Error AlreadyExists = Error.Conflict(
        "FundingBilling.BillingAccount.AlreadyExists",
        "errors.fundingBilling.billingAccount.alreadyExists");

    public static readonly Error RestrictionNotAllowed = Error.Conflict(
        "FundingBilling.BillingAccount.Restriction.NotAllowed",
        "errors.fundingBilling.billingAccount.restriction.notAllowed");

    public static readonly Error SuspensionNotAllowed = Error.Conflict(
        "FundingBilling.BillingAccount.Suspension.NotAllowed",
        "errors.fundingBilling.billingAccount.suspension.notAllowed");

    public static readonly Error ReactivationNotAllowed = Error.Conflict(
        "FundingBilling.BillingAccount.Reactivation.NotAllowed",
        "errors.fundingBilling.billingAccount.reactivation.notAllowed");

    public static readonly Error ClosureNotAllowed = Error.Conflict(
        "FundingBilling.BillingAccount.Closure.NotAllowed",
        "errors.fundingBilling.billingAccount.closure.notAllowed");

    public static readonly Error ClosedAccountOperationNotAllowed = Error.Conflict(
        "FundingBilling.BillingAccount.Closed.OperationNotAllowed",
        "errors.fundingBilling.billingAccount.closed.operationNotAllowed");

    public static readonly Error CurrencyMismatch = Error.Validation(
        "FundingBilling.BillingAccount.Currency.Mismatch",
        "errors.fundingBilling.billingAccount.currency.mismatch");

    public static readonly Error InvalidFinancialOperation = Error.Validation(
        "FundingBilling.BillingAccount.FinancialOperation.Invalid",
        "errors.fundingBilling.billingAccount.financialOperation.invalid");
}
