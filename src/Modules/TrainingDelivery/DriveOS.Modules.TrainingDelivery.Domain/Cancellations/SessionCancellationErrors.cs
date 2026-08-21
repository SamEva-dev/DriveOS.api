using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Cancellations;

public static class SessionCancellationErrors
{
    public static readonly Error Invalid = Error.Validation("TrainingDelivery.Cancellation.Invalid", "errors.trainingDelivery.cancellation.invalid");
    public static readonly Error NotFound = Error.NotFound("TrainingDelivery.Cancellation.NotFound", "errors.trainingDelivery.cancellation.notFound");
    public static readonly Error RequiresStartedSession = Error.Conflict("TrainingDelivery.Cancellation.RequiresStartedSession", "errors.trainingDelivery.cancellation.requiresStartedSession");
    public static readonly Error UseSchedulingBeforeStart = Error.Conflict("TrainingDelivery.Cancellation.UseSchedulingBeforeStart", "errors.trainingDelivery.cancellation.useSchedulingBeforeStart");
    public static readonly Error SessionAlreadyCompleted = Error.Conflict("TrainingDelivery.Cancellation.SessionAlreadyCompleted", "errors.trainingDelivery.cancellation.sessionAlreadyCompleted");
    public static readonly Error AlreadyCancelled = Error.Conflict("TrainingDelivery.Cancellation.AlreadyCancelled", "errors.trainingDelivery.cancellation.alreadyCancelled");
    public static readonly Error OperationConflict = Error.Conflict("TrainingDelivery.Cancellation.Operation.Conflict", "errors.trainingDelivery.cancellation.operation.conflict");
    public static readonly Error CancelledAtInvalid = Error.Validation("TrainingDelivery.Cancellation.CancelledAt.Invalid", "errors.trainingDelivery.cancellation.cancelledAt.invalid");
    public static readonly Error ReasonDetailsTooLong = Error.Validation("TrainingDelivery.Cancellation.ReasonDetails.TooLong", "errors.trainingDelivery.cancellation.reasonDetails.tooLong");
    public static readonly Error DecisionReasonTooLong = Error.Validation("TrainingDelivery.Cancellation.DecisionReason.TooLong", "errors.trainingDelivery.cancellation.decisionReason.tooLong");
    public static readonly Error CreditDecisionInvalid = Error.Validation("TrainingDelivery.Cancellation.CreditDecision.Invalid", "errors.trainingDelivery.cancellation.creditDecision.invalid");
    public static readonly Error ProviderDecisionInvalid = Error.Validation("TrainingDelivery.Cancellation.ProviderDecision.Invalid", "errors.trainingDelivery.cancellation.providerDecision.invalid");
    public static readonly Error BillingDecisionInvalid = Error.Validation("TrainingDelivery.Cancellation.BillingDecision.Invalid", "errors.trainingDelivery.cancellation.billingDecision.invalid");
}
