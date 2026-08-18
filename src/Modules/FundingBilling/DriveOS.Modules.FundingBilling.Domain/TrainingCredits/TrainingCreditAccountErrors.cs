using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits;

public static class TrainingCreditAccountErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("FundingBilling.TrainingCreditAccount.Id.Invalid", "errors.fundingBilling.trainingCredits.id.invalid");
    public static readonly Error InvalidOwner = Error.Validation("FundingBilling.TrainingCreditAccount.Owner.Invalid", "errors.fundingBilling.trainingCredits.owner.invalid");
    public static readonly Error InvalidCreditType = Error.Validation("FundingBilling.TrainingCreditAccount.CreditType.Invalid", "errors.fundingBilling.trainingCredits.creditType.invalid");
    public static readonly Error InvalidExpirationDate = Error.Validation("FundingBilling.TrainingCreditAccount.ExpirationDate.Invalid", "errors.fundingBilling.trainingCredits.expirationDate.invalid");
    public static readonly Error Duplicate = Error.Conflict("FundingBilling.TrainingCreditAccount.Duplicate", "errors.fundingBilling.trainingCredits.duplicate");
    public static readonly Error NotFound = Error.NotFound("FundingBilling.TrainingCreditAccount.NotFound", "errors.fundingBilling.trainingCredits.notFound");
    public static readonly Error MovementInvalid = Error.Validation("FundingBilling.TrainingCreditAccount.Movement.Invalid", "errors.fundingBilling.trainingCredits.movement.invalid");
    public static readonly Error MovementReferenceDuplicate = Error.Conflict("FundingBilling.TrainingCreditAccount.Movement.ReferenceDuplicate", "errors.fundingBilling.trainingCredits.movement.referenceDuplicate");
    public static readonly Error OperationNotAllowed = Error.Conflict("FundingBilling.TrainingCreditAccount.Operation.NotAllowed", "errors.fundingBilling.trainingCredits.operation.notAllowed");
    public static readonly Error InsufficientAvailable = Error.Conflict("FundingBilling.TrainingCreditAccount.Available.Insufficient", "errors.fundingBilling.trainingCredits.available.insufficient");
    public static readonly Error InsufficientReserved = Error.Conflict("FundingBilling.TrainingCreditAccount.Reserved.Insufficient", "errors.fundingBilling.trainingCredits.reserved.insufficient");
    public static readonly Error AdjustmentWouldOverdraw = Error.Conflict("FundingBilling.TrainingCreditAccount.Adjustment.Overdraw", "errors.fundingBilling.trainingCredits.adjustment.overdraw");
    public static readonly Error BillingAccountNotFound = Error.NotFound("FundingBilling.TrainingCreditAccount.BillingAccount.NotFound", "errors.fundingBilling.trainingCredits.billingAccount.notFound");
}
