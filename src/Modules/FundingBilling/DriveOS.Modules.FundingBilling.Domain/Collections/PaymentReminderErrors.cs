using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.Collections;

public static class PaymentReminderErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.PaymentReminder.Id.Invalid", "errors.fundingBilling.reminder.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.PaymentReminder.Owner.Invalid", "errors.fundingBilling.reminder.owner.invalid");
    public static readonly Error InvalidTarget = Error.Validation("FundingBilling.PaymentReminder.Target.Invalid", "errors.fundingBilling.reminder.target.invalid");
    public static readonly Error InvalidAmount = Error.Validation("FundingBilling.PaymentReminder.Amount.Invalid", "errors.fundingBilling.reminder.amount.invalid");
    public static readonly Error InvalidCurrency = Error.Validation("FundingBilling.PaymentReminder.Currency.Invalid", "errors.fundingBilling.reminder.currency.invalid");
    public static readonly Error InvalidDueDate = Error.Validation("FundingBilling.PaymentReminder.DueDate.Invalid", "errors.fundingBilling.reminder.dueDate.invalid");
    public static readonly Error DuplicatePending = Error.Conflict("FundingBilling.PaymentReminder.Pending.Exists", "errors.fundingBilling.reminder.pending.exists");
    public static readonly Error InvalidStatus = Error.Conflict("FundingBilling.PaymentReminder.Status.Invalid", "errors.fundingBilling.reminder.status.invalid");
    public static readonly Error InvalidEmailMessage = Error.Validation("FundingBilling.PaymentReminder.EmailMessage.Invalid", "errors.fundingBilling.reminder.emailMessage.invalid");
}
