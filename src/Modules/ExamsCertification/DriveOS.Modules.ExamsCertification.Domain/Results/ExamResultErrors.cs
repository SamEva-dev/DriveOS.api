using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Results;

public static class ExamResultErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Result.NotFound", "errors.exams.result.notFound");
    public static readonly Error AttemptNotFound = Error.NotFound("Exams.Result.AttemptNotFound", "errors.exams.result.attemptNotFound");
    public static readonly Error AttemptNotAwaitingResult = Error.Conflict("Exams.Result.AttemptNotAwaitingResult", "errors.exams.result.attemptNotAwaitingResult");
    public static readonly Error AlreadyExists = Error.Conflict("Exams.Result.AlreadyExists", "errors.exams.result.alreadyExists");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Result.InvalidIdentifier", "errors.exams.result.invalidIdentifier");
    public static readonly Error InvalidOutcome = Error.Validation("Exams.Result.InvalidOutcome", "errors.exams.result.invalidOutcome");
    public static readonly Error InvalidSource = Error.Validation("Exams.Result.InvalidSource", "errors.exams.result.invalidSource");
    public static readonly Error InvalidScore = Error.Validation("Exams.Result.InvalidScore", "errors.exams.result.invalidScore");
    public static readonly Error InvalidTransition = Error.Conflict("Exams.Result.InvalidTransition", "errors.exams.result.invalidTransition");
    public static readonly Error VerificationEvidenceRequired = Error.Validation("Exams.Result.VerificationEvidenceRequired", "errors.exams.result.verificationEvidenceRequired");
    public static readonly Error CorrectionReasonRequired = Error.Validation("Exams.Result.CorrectionReasonRequired", "errors.exams.result.correctionReasonRequired");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Result.OperationConflict", "errors.exams.result.operationConflict");
    public static readonly Error InvalidOperation = Error.Validation("Exams.Result.InvalidOperation", "errors.exams.result.invalidOperation");
}
