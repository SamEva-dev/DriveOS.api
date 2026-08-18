using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices;

public static class InvoiceErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "FundingBilling.Invoice.Id.Invalid",
        "errors.fundingBilling.invoice.id.invalid");

    public static readonly Error InvalidOwner = Error.Validation(
        "FundingBilling.Invoice.Owner.Invalid",
        "errors.fundingBilling.invoice.owner.invalid");

    public static readonly Error InvalidCurrency = Error.Validation(
        "FundingBilling.Invoice.Currency.Invalid",
        "errors.fundingBilling.invoice.currency.invalid");

    public static readonly Error InvalidLine = Error.Validation(
        "FundingBilling.Invoice.Line.Invalid",
        "errors.fundingBilling.invoice.line.invalid");

    public static readonly Error LineNotFound = Error.NotFound(
        "FundingBilling.Invoice.Line.NotFound",
        "errors.fundingBilling.invoice.line.notFound");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "FundingBilling.Invoice.Modification.NotAllowed",
        "errors.fundingBilling.invoice.modification.notAllowed");

    public static readonly Error EmptyInvoice = Error.Validation(
        "FundingBilling.Invoice.Empty",
        "errors.fundingBilling.invoice.empty");

    public static readonly Error InvalidInvoiceNumber = Error.Validation(
        "FundingBilling.Invoice.Number.Invalid",
        "errors.fundingBilling.invoice.number.invalid");

    public static readonly Error InvalidIssuePeriod = Error.Validation(
        "FundingBilling.Invoice.IssuePeriod.Invalid",
        "errors.fundingBilling.invoice.issuePeriod.invalid");

    public static readonly Error IssueNotAllowed = Error.Conflict(
        "FundingBilling.Invoice.Issue.NotAllowed",
        "errors.fundingBilling.invoice.issue.notAllowed");

    public static readonly Error InvalidActor = Error.Validation(
        "FundingBilling.Invoice.Actor.Invalid",
        "errors.fundingBilling.invoice.actor.invalid");

    public static readonly Error CurrencyMismatch = Error.Validation(
        "FundingBilling.Invoice.Currency.Mismatch", "errors.fundingBilling.invoice.currency.mismatch");
    public static readonly Error PaymentAllocationNotAllowed = Error.Conflict(
        "FundingBilling.Invoice.PaymentAllocation.NotAllowed", "errors.fundingBilling.invoice.paymentAllocation.notAllowed");
    public static readonly Error PaymentAllocationAmountExceeded = Error.Conflict(
        "FundingBilling.Invoice.PaymentAllocation.AmountExceeded", "errors.fundingBilling.invoice.paymentAllocation.amountExceeded");
    public static readonly Error NotFound = Error.NotFound(
        "FundingBilling.Invoice.NotFound",
        "errors.fundingBilling.invoice.notFound");

    public static readonly Error BillingAccountNotFound = Error.NotFound(
        "FundingBilling.Invoice.BillingAccount.NotFound",
        "errors.fundingBilling.invoice.billingAccount.notFound");

    public static readonly Error BillingAccountClosed = Error.Conflict(
        "FundingBilling.Invoice.BillingAccount.Closed",
        "errors.fundingBilling.invoice.billingAccount.closed");
    public static readonly Error OverdueNotAllowed = Error.Conflict(
        "FundingBilling.Invoice.Overdue.NotAllowed", "errors.fundingBilling.invoice.overdue.notAllowed");
    public static readonly Error NotYetOverdue = Error.Conflict(
        "FundingBilling.Invoice.Overdue.NotDue", "errors.fundingBilling.invoice.overdue.notDue");
    public static readonly Error CreditNoteNotAllowed = Error.Conflict(
        "FundingBilling.Invoice.CreditNote.NotAllowed", "errors.fundingBilling.invoice.creditNote.notAllowed");
    public static readonly Error CreditNoteAmountExceeded = Error.Conflict(
        "FundingBilling.Invoice.CreditNote.AmountExceeded", "errors.fundingBilling.invoice.creditNote.amountExceeded");
}


