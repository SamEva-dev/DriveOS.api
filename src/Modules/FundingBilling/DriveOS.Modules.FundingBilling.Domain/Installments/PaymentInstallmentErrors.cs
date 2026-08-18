using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Installments;

public static class PaymentInstallmentErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "FundingBilling.PaymentInstallment.Id.Invalid",
        "errors.fundingBilling.installment.id.invalid");

    public static readonly Error InvalidOwner = Error.Validation(
        "FundingBilling.PaymentInstallment.Owner.Invalid",
        "errors.fundingBilling.installment.owner.invalid");

    public static readonly Error InvalidAmount = Error.Validation(
        "FundingBilling.PaymentInstallment.Amount.Invalid",
        "errors.fundingBilling.installment.amount.invalid");

    public static readonly Error InvalidCurrency = Error.Validation(
        "FundingBilling.PaymentInstallment.Currency.Invalid",
        "errors.fundingBilling.installment.currency.invalid");

    public static readonly Error InvalidDueDate = Error.Validation(
        "FundingBilling.PaymentInstallment.DueDate.Invalid",
        "errors.fundingBilling.installment.dueDate.invalid");

    public static readonly Error InvalidFinancingParty = Error.Validation(
        "FundingBilling.PaymentInstallment.FinancingParty.Invalid",
        "errors.fundingBilling.installment.financingParty.invalid");

    public static readonly Error NotFound = Error.NotFound(
        "FundingBilling.PaymentInstallment.NotFound",
        "errors.fundingBilling.installment.notFound");

    public static readonly Error BillingAccountNotFound = Error.NotFound(
        "FundingBilling.PaymentInstallment.BillingAccount.NotFound",
        "errors.fundingBilling.installment.billingAccount.notFound");

    public static readonly Error BillingAccountClosed = Error.Conflict(
        "FundingBilling.PaymentInstallment.BillingAccount.Closed",
        "errors.fundingBilling.installment.billingAccount.closed");

    public static readonly Error CurrencyMismatch = Error.Validation(
        "FundingBilling.PaymentInstallment.Currency.Mismatch",
        "errors.fundingBilling.installment.currency.mismatch");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "FundingBilling.PaymentInstallment.Modification.NotAllowed",
        "errors.fundingBilling.installment.modification.notAllowed");

    public static readonly Error InvalidReason = Error.Validation(
        "FundingBilling.PaymentInstallment.Reason.Invalid",
        "errors.fundingBilling.installment.reason.invalid");

    public static readonly Error InvalidActor = Error.Validation(
        "FundingBilling.PaymentInstallment.Actor.Invalid",
        "errors.fundingBilling.installment.actor.invalid");

    public static readonly Error PaymentAllocationNotAllowed = Error.Conflict(
        "FundingBilling.PaymentInstallment.PaymentAllocation.NotAllowed",
        "errors.fundingBilling.installment.paymentAllocation.notAllowed");

    public static readonly Error PaymentAllocationAmountExceeded = Error.Conflict(
        "FundingBilling.PaymentInstallment.PaymentAllocation.AmountExceeded",
        "errors.fundingBilling.installment.paymentAllocation.amountExceeded");

    public static readonly Error ScheduleEmpty = Error.Validation(
        "FundingBilling.PaymentInstallment.Schedule.Empty",
        "errors.fundingBilling.installment.schedule.empty");
    public static readonly Error OverdueNotAllowed = Error.Conflict(
        "FundingBilling.PaymentInstallment.Overdue.NotAllowed", "errors.fundingBilling.installment.overdue.notAllowed");
    public static readonly Error NotYetOverdue = Error.Conflict(
        "FundingBilling.PaymentInstallment.Overdue.NotDue", "errors.fundingBilling.installment.overdue.notDue");
}

