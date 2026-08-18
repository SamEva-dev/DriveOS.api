using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.CreditNotes;

public static class CreditNoteErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.CreditNote.Id.Invalid", "errors.fundingBilling.creditNote.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.CreditNote.Owner.Invalid", "errors.fundingBilling.creditNote.owner.invalid");
    public static readonly Error InvalidCurrency = Error.Validation("FundingBilling.CreditNote.Currency.Invalid", "errors.fundingBilling.creditNote.currency.invalid");
    public static readonly Error InvalidReason = Error.Validation("FundingBilling.CreditNote.Reason.Invalid", "errors.fundingBilling.creditNote.reason.invalid");
    public static readonly Error InvalidLine = Error.Validation("FundingBilling.CreditNote.Line.Invalid", "errors.fundingBilling.creditNote.line.invalid");
    public static readonly Error LineNotFound = Error.NotFound("FundingBilling.CreditNote.Line.NotFound", "errors.fundingBilling.creditNote.line.notFound");
    public static readonly Error ModificationNotAllowed = Error.Conflict("FundingBilling.CreditNote.Modification.NotAllowed", "errors.fundingBilling.creditNote.modification.notAllowed");
    public static readonly Error Empty = Error.Validation("FundingBilling.CreditNote.Empty", "errors.fundingBilling.creditNote.empty");
    public static readonly Error InvalidNumber = Error.Validation("FundingBilling.CreditNote.Number.Invalid", "errors.fundingBilling.creditNote.number.invalid");
    public static readonly Error IssueNotAllowed = Error.Conflict("FundingBilling.CreditNote.Issue.NotAllowed", "errors.fundingBilling.creditNote.issue.notAllowed");
    public static readonly Error AmountExceeded = Error.Conflict("FundingBilling.CreditNote.Amount.Exceeded", "errors.fundingBilling.creditNote.amount.exceeded");
    public static readonly Error InvalidActor = Error.Validation("FundingBilling.CreditNote.Actor.Invalid", "errors.fundingBilling.creditNote.actor.invalid");
    public static readonly Error NotFound = Error.NotFound("FundingBilling.CreditNote.NotFound", "errors.fundingBilling.creditNote.notFound");
    public static readonly Error InvoiceNotFound = Error.NotFound("FundingBilling.CreditNote.Invoice.NotFound", "errors.fundingBilling.creditNote.invoice.notFound");
    public static readonly Error InvoiceNotCreditable = Error.Conflict("FundingBilling.CreditNote.Invoice.NotCreditable", "errors.fundingBilling.creditNote.invoice.notCreditable");
    public static readonly Error BillingAccountNotFound = Error.NotFound("FundingBilling.CreditNote.BillingAccount.NotFound", "errors.fundingBilling.creditNote.billingAccount.notFound");
}
