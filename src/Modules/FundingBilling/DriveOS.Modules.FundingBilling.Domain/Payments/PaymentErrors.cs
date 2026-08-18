using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "FundingBilling.Payment.Id.Invalid", "errors.fundingBilling.payment.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation(
        "FundingBilling.Payment.Owner.Invalid", "errors.fundingBilling.payment.owner.invalid");
    public static readonly Error InvalidPayer = Error.Validation(
        "FundingBilling.Payment.Payer.Invalid", "errors.fundingBilling.payment.payer.invalid");
    public static readonly Error InvalidAmount = Error.Validation(
        "FundingBilling.Payment.Amount.Invalid", "errors.fundingBilling.payment.amount.invalid");
    public static readonly Error InvalidCurrency = Error.Validation(
        "FundingBilling.Payment.Currency.Invalid", "errors.fundingBilling.payment.currency.invalid");
    public static readonly Error InvalidPaymentMethod = Error.Validation(
        "FundingBilling.Payment.Method.Invalid", "errors.fundingBilling.payment.method.invalid");
    public static readonly Error InvalidExternalReference = Error.Validation(
        "FundingBilling.Payment.ExternalReference.Invalid", "errors.fundingBilling.payment.externalReference.invalid");
    public static readonly Error InvalidActor = Error.Validation(
        "FundingBilling.Payment.Actor.Invalid", "errors.fundingBilling.payment.actor.invalid");
    public static readonly Error InvalidFailureReason = Error.Validation(
        "FundingBilling.Payment.FailureReason.Invalid", "errors.fundingBilling.payment.failureReason.invalid");
    public static readonly Error ProcessingNotAllowed = Error.Conflict(
        "FundingBilling.Payment.Processing.NotAllowed", "errors.fundingBilling.payment.processing.notAllowed");
    public static readonly Error PaidNotAllowed = Error.Conflict(
        "FundingBilling.Payment.Paid.NotAllowed", "errors.fundingBilling.payment.paid.notAllowed");
    public static readonly Error FailureNotAllowed = Error.Conflict(
        "FundingBilling.Payment.Failure.NotAllowed", "errors.fundingBilling.payment.failure.notAllowed");
    public static readonly Error CancellationNotAllowed = Error.Conflict(
        "FundingBilling.Payment.Cancellation.NotAllowed", "errors.fundingBilling.payment.cancellation.notAllowed");
    public static readonly Error BillingAccountNotFound = Error.NotFound(
        "FundingBilling.Payment.BillingAccount.NotFound", "errors.fundingBilling.payment.billingAccount.notFound");
    public static readonly Error BillingAccountClosed = Error.Conflict(
        "FundingBilling.Payment.BillingAccount.Closed", "errors.fundingBilling.payment.billingAccount.closed");
    public static readonly Error CurrencyMismatch = Error.Validation(
        "FundingBilling.Payment.Currency.Mismatch", "errors.fundingBilling.payment.currency.mismatch");
    public static readonly Error ExternalReferenceAlreadyUsed = Error.Conflict(
        "FundingBilling.Payment.ExternalReference.AlreadyUsed", "errors.fundingBilling.payment.externalReference.alreadyUsed");
    public static readonly Error AllocationInvalid = Error.Validation(
        "FundingBilling.Payment.Allocation.Invalid", "errors.fundingBilling.payment.allocation.invalid");
    public static readonly Error AllocationTargetInvalid = Error.Validation(
        "FundingBilling.Payment.Allocation.Target.Invalid", "errors.fundingBilling.payment.allocation.target.invalid");
    public static readonly Error AllocationPaymentNotPaid = Error.Conflict(
        "FundingBilling.Payment.Allocation.PaymentNotPaid", "errors.fundingBilling.payment.allocation.paymentNotPaid");
    public static readonly Error AllocationAmountExceeded = Error.Conflict(
        "FundingBilling.Payment.Allocation.AmountExceeded", "errors.fundingBilling.payment.allocation.amountExceeded");
    public static readonly Error AllocationTargetNotFound = Error.NotFound(
        "FundingBilling.Payment.Allocation.Target.NotFound", "errors.fundingBilling.payment.allocation.target.notFound");
    public static readonly Error AllocationTargetInvalidState = Error.Conflict(
        "FundingBilling.Payment.Allocation.Target.InvalidState", "errors.fundingBilling.payment.allocation.target.invalidState");
    public static readonly Error AllocationTargetAmountExceeded = Error.Conflict(
        "FundingBilling.Payment.Allocation.Target.AmountExceeded", "errors.fundingBilling.payment.allocation.target.amountExceeded");
    public static readonly Error AllocationBillingAccountMismatch = Error.Validation(
        "FundingBilling.Payment.Allocation.BillingAccountMismatch", "errors.fundingBilling.payment.allocation.billingAccountMismatch");
    public static readonly Error NotFound = Error.NotFound(
        "FundingBilling.Payment.NotFound", "errors.fundingBilling.payment.notFound");
    public static readonly Error RefundNotAllowed = Error.Conflict("FundingBilling.Payment.Refund.NotAllowed", "errors.fundingBilling.payment.refund.notAllowed");
    public static readonly Error RefundAmountExceeded = Error.Conflict("FundingBilling.Payment.Refund.AmountExceeded", "errors.fundingBilling.payment.refund.amountExceeded");
    public static readonly Error PayerNotAuthorized = Error.Conflict("FundingBilling.Payment.Payer.NotAuthorized", "errors.fundingBilling.payment.payer.notAuthorized");
}
