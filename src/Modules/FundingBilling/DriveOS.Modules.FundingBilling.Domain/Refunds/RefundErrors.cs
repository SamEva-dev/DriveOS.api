using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Refunds;

public static class RefundErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.Refund.Id.Invalid", "errors.fundingBilling.refund.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.Refund.Owner.Invalid", "errors.fundingBilling.refund.owner.invalid");
    public static readonly Error InvalidAmount = Error.Validation("FundingBilling.Refund.Amount.Invalid", "errors.fundingBilling.refund.amount.invalid");
    public static readonly Error InvalidCurrency = Error.Validation("FundingBilling.Refund.Currency.Invalid", "errors.fundingBilling.refund.currency.invalid");
    public static readonly Error InvalidReason = Error.Validation("FundingBilling.Refund.Reason.Invalid", "errors.fundingBilling.refund.reason.invalid");
    public static readonly Error InvalidActor = Error.Validation("FundingBilling.Refund.Actor.Invalid", "errors.fundingBilling.refund.actor.invalid");
    public static readonly Error InvalidProviderReference = Error.Validation("FundingBilling.Refund.ProviderReference.Invalid", "errors.fundingBilling.refund.providerReference.invalid");
    public static readonly Error NotFound = Error.NotFound("FundingBilling.Refund.NotFound", "errors.fundingBilling.refund.notFound");
    public static readonly Error PaymentNotFound = Error.NotFound("FundingBilling.Refund.Payment.NotFound", "errors.fundingBilling.refund.payment.notFound");
    public static readonly Error BillingAccountNotFound = Error.NotFound("FundingBilling.Refund.BillingAccount.NotFound", "errors.fundingBilling.refund.billingAccount.notFound");
    public static readonly Error PaymentNotRefundable = Error.Conflict("FundingBilling.Refund.Payment.NotRefundable", "errors.fundingBilling.refund.payment.notRefundable");
    public static readonly Error AmountExceeded = Error.Conflict("FundingBilling.Refund.Amount.Exceeded", "errors.fundingBilling.refund.amount.exceeded");
    public static readonly Error CurrencyMismatch = Error.Validation("FundingBilling.Refund.Currency.Mismatch", "errors.fundingBilling.refund.currency.mismatch");
    public static readonly Error ApprovalNotAllowed = Error.Conflict("FundingBilling.Refund.Approval.NotAllowed", "errors.fundingBilling.refund.approval.notAllowed");
    public static readonly Error ProcessingNotAllowed = Error.Conflict("FundingBilling.Refund.Processing.NotAllowed", "errors.fundingBilling.refund.processing.notAllowed");
    public static readonly Error CompletionNotAllowed = Error.Conflict("FundingBilling.Refund.Completion.NotAllowed", "errors.fundingBilling.refund.completion.notAllowed");
    public static readonly Error RejectionNotAllowed = Error.Conflict("FundingBilling.Refund.Rejection.NotAllowed", "errors.fundingBilling.refund.rejection.notAllowed");
    public static readonly Error FailureNotAllowed = Error.Conflict("FundingBilling.Refund.Failure.NotAllowed", "errors.fundingBilling.refund.failure.notAllowed");
    public static readonly Error CancellationNotAllowed = Error.Conflict("FundingBilling.Refund.Cancellation.NotAllowed", "errors.fundingBilling.refund.cancellation.notAllowed");
}
