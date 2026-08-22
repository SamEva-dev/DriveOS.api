using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure;

public static class ExamFailureAnalysisErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.FailureAnalysis.NotFound", "errors.exams.failureAnalysis.notFound");
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.FailureAnalysis.InvalidIdentifier", "errors.exams.failureAnalysis.invalidIdentifier");
    public static readonly Error InvalidFinding = Error.Validation("Exams.FailureAnalysis.InvalidFinding", "errors.exams.failureAnalysis.invalidFinding");
    public static readonly Error DuplicateFinding = Error.Conflict("Exams.FailureAnalysis.DuplicateFinding", "errors.exams.failureAnalysis.duplicateFinding");
    public static readonly Error AlreadyCompleted = Error.Conflict("Exams.FailureAnalysis.AlreadyCompleted", "errors.exams.failureAnalysis.alreadyCompleted");
    public static readonly Error Superseded = Error.Conflict("Exams.FailureAnalysis.Superseded", "errors.exams.failureAnalysis.superseded");
    public static readonly Error FindingsRequired = Error.Validation("Exams.FailureAnalysis.FindingsRequired", "errors.exams.failureAnalysis.findingsRequired");
    public static readonly Error InvalidRecommendedHours = Error.Validation("Exams.FailureAnalysis.InvalidRecommendedHours", "errors.exams.failureAnalysis.invalidRecommendedHours");
    public static readonly Error NotSubmitted = Error.Conflict("Exams.FailureAnalysis.NotSubmitted", "errors.exams.failureAnalysis.notSubmitted");
    public static readonly Error SummaryRequired = Error.Validation("Exams.FailureAnalysis.SummaryRequired", "errors.exams.failureAnalysis.summaryRequired");
}
