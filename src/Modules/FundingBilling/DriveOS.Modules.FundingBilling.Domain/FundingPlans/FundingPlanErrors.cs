using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.FundingPlans;

public static class FundingPlanErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.FundingPlan.Id.Invalid", "errors.fundingBilling.fundingPlan.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.FundingPlan.Owner.Invalid", "errors.fundingBilling.fundingPlan.owner.invalid");
    public static readonly Error InvalidContract = Error.Validation("FundingBilling.FundingPlan.Contract.Invalid", "errors.fundingBilling.fundingPlan.contract.invalid");
    public static readonly Error InvalidAmount = Error.Validation("FundingBilling.FundingPlan.Amount.Invalid", "errors.fundingBilling.fundingPlan.amount.invalid");
    public static readonly Error InvalidCurrency = Error.Validation("FundingBilling.FundingPlan.Currency.Invalid", "errors.fundingBilling.fundingPlan.currency.invalid");
    public static readonly Error InvalidFinancingParty = Error.Validation("FundingBilling.FundingPlan.FinancingParty.Invalid", "errors.fundingBilling.fundingPlan.financingParty.invalid");
    public static readonly Error InvalidReference = Error.Validation("FundingBilling.FundingPlan.Reference.Invalid", "errors.fundingBilling.fundingPlan.reference.invalid");
    public static readonly Error AllocationExceeded = Error.Conflict("FundingBilling.FundingPlan.Allocation.Exceeded", "errors.fundingBilling.fundingPlan.allocation.exceeded");
    public static readonly Error CoverageIncomplete = Error.Conflict("FundingBilling.FundingPlan.Coverage.Incomplete", "errors.fundingBilling.fundingPlan.coverage.incomplete");
    public static readonly Error ModificationNotAllowed = Error.Conflict("FundingBilling.FundingPlan.Modification.NotAllowed", "errors.fundingBilling.fundingPlan.modification.notAllowed");
    public static readonly Error ApprovalNotAllowed = Error.Conflict("FundingBilling.FundingPlan.Approval.NotAllowed", "errors.fundingBilling.fundingPlan.approval.notAllowed");
    public static readonly Error AllocationNotFound = Error.NotFound("FundingBilling.FundingPlan.Allocation.NotFound", "errors.fundingBilling.fundingPlan.allocation.notFound");
    public static readonly Error InvalidActor = Error.Validation("FundingBilling.FundingPlan.Actor.Invalid", "errors.fundingBilling.fundingPlan.actor.invalid");
    public static readonly Error InvalidReason = Error.Validation("FundingBilling.FundingPlan.Reason.Invalid", "errors.fundingBilling.fundingPlan.reason.invalid");
    public static readonly Error BillingAccountNotFound = Error.NotFound("FundingBilling.FundingPlan.BillingAccount.NotFound", "errors.fundingBilling.fundingPlan.billingAccount.notFound");
    public static readonly Error BillingAccountClosed = Error.Conflict("FundingBilling.FundingPlan.BillingAccount.Closed", "errors.fundingBilling.fundingPlan.billingAccount.closed");
    public static readonly Error CurrencyMismatch = Error.Validation("FundingBilling.FundingPlan.Currency.Mismatch", "errors.fundingBilling.fundingPlan.currency.mismatch");
    public static readonly Error AlreadyExistsForContract = Error.Conflict("FundingBilling.FundingPlan.Contract.AlreadyExists", "errors.fundingBilling.fundingPlan.contract.alreadyExists");
    public static readonly Error NotFound = Error.NotFound("FundingBilling.FundingPlan.NotFound", "errors.fundingBilling.fundingPlan.notFound");
    public static readonly Error FunderNotAuthorized = Error.Conflict("FundingBilling.FundingPlan.Funder.NotAuthorized", "errors.fundingBilling.fundingPlan.funder.notAuthorized");
}
