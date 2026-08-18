using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.BillingParties;

public static class BillingPartyErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.BillingParty.Id.Invalid", "errors.fundingBilling.billingParty.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.BillingParty.Owner.Invalid", "errors.fundingBilling.billingParty.owner.invalid");
    public static readonly Error InvalidParty = Error.Validation("FundingBilling.BillingParty.Party.Invalid", "errors.fundingBilling.billingParty.party.invalid");
    public static readonly Error InvalidRole = Error.Validation("FundingBilling.BillingParty.Role.Invalid", "errors.fundingBilling.billingParty.role.invalid");
    public static readonly Error InvalidPeriod = Error.Validation("FundingBilling.BillingParty.Period.Invalid", "errors.fundingBilling.billingParty.period.invalid");
    public static readonly Error InvalidAmount = Error.Validation("FundingBilling.BillingParty.Amount.Invalid", "errors.fundingBilling.billingParty.amount.invalid");
    public static readonly Error InvalidPriority = Error.Validation("FundingBilling.BillingParty.Priority.Invalid", "errors.fundingBilling.billingParty.priority.invalid");
    public static readonly Error InvalidActor = Error.Validation("FundingBilling.BillingParty.Actor.Invalid", "errors.fundingBilling.billingParty.actor.invalid");
    public static readonly Error InvalidReason = Error.Validation("FundingBilling.BillingParty.Reason.Invalid", "errors.fundingBilling.billingParty.reason.invalid");
    public static readonly Error AlreadyEnded = Error.Conflict("FundingBilling.BillingParty.AlreadyEnded", "errors.fundingBilling.billingParty.alreadyEnded");
    public static readonly Error Duplicate = Error.Conflict("FundingBilling.BillingParty.Duplicate", "errors.fundingBilling.billingParty.duplicate");
    public static readonly Error NotFound = Error.NotFound("FundingBilling.BillingParty.NotFound", "errors.fundingBilling.billingParty.notFound");
    public static readonly Error BillingAccountNotFound = Error.NotFound("FundingBilling.BillingParty.BillingAccount.NotFound", "errors.fundingBilling.billingParty.billingAccount.notFound");
}
