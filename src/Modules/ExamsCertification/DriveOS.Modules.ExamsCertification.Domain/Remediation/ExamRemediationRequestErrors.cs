using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Remediation;

public static class ExamRemediationRequestErrors
{
    public static readonly Error NotFound = Error.NotFound("Exams.Remediation.NotFound", "errors.exams.remediation.notFound");
    public static readonly Error InvalidContext = Error.Validation("Exams.Remediation.InvalidContext", "errors.exams.remediation.invalidContext");
    public static readonly Error AnalysisNotApproved = Error.Conflict("Exams.Remediation.AnalysisNotApproved", "errors.exams.remediation.analysisNotApproved");
    public static readonly Error AlreadyExists = Error.Conflict("Exams.Remediation.AlreadyExists", "errors.exams.remediation.alreadyExists");
    public static readonly Error ConfigurationRequired = Error.Validation("Exams.Remediation.ConfigurationRequired", "errors.exams.remediation.configurationRequired");
    public static readonly Error InvalidReviewDate = Error.Validation("Exams.Remediation.InvalidReviewDate", "errors.exams.remediation.invalidReviewDate");
    public static readonly Error InvalidRecommendedHours = Error.Validation("Exams.Remediation.InvalidRecommendedHours", "errors.exams.remediation.invalidRecommendedHours");
    public static readonly Error ProvisionNotAllowed = Error.Conflict("Exams.Remediation.ProvisionNotAllowed", "errors.exams.remediation.provisionNotAllowed");
    public static readonly Error ValidationNotAllowed = Error.Conflict("Exams.Remediation.ValidationNotAllowed", "errors.exams.remediation.validationNotAllowed");
    public static readonly Error Superseded = Error.Conflict("Exams.Remediation.Superseded", "errors.exams.remediation.superseded");
    public static readonly Error CancelNotAllowed = Error.Conflict("Exams.Remediation.CancelNotAllowed", "errors.exams.remediation.cancelNotAllowed");
}
