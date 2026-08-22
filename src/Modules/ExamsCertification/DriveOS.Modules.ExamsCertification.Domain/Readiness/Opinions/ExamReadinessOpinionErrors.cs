using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;

public static class ExamReadinessOpinionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Readiness.Opinion.InvalidIdentifier", "errors.exams.readiness.opinion.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.Readiness.Opinion.InvalidOrganization", "errors.exams.readiness.opinion.invalidOrganization");
    public static readonly Error InvalidStudent = Error.Validation("Exams.Readiness.Opinion.InvalidStudent", "errors.exams.readiness.opinion.invalidStudent");
    public static readonly Error InvalidTrainingPath = Error.Validation("Exams.Readiness.Opinion.InvalidTrainingPath", "errors.exams.readiness.opinion.invalidTrainingPath");
    public static readonly Error InvalidAuthor = Error.Validation("Exams.Readiness.Opinion.InvalidAuthor", "errors.exams.readiness.opinion.invalidAuthor");
    public static readonly Error InvalidOperation = Error.Validation("Exams.Readiness.Opinion.InvalidOperation", "errors.exams.readiness.opinion.invalidOperation");
    public static readonly Error InvalidVersion = Error.Validation("Exams.Readiness.Opinion.InvalidVersion", "errors.exams.readiness.opinion.invalidVersion");
    public static readonly Error InvalidOpinion = Error.Validation("Exams.Readiness.Opinion.InvalidOpinion", "errors.exams.readiness.opinion.invalidOpinion");
    public static readonly Error InvalidAutonomy = Error.Validation("Exams.Readiness.Opinion.InvalidAutonomy", "errors.exams.readiness.opinion.invalidAutonomy");
    public static readonly Error InvalidComment = Error.Validation("Exams.Readiness.Opinion.InvalidComment", "errors.exams.readiness.opinion.invalidComment");
    public static readonly Error ReservationsRequired = Error.Validation("Exams.Readiness.Opinion.ReservationsRequired", "errors.exams.readiness.opinion.reservationsRequired");
    public static readonly Error ConditionsRequired = Error.Validation("Exams.Readiness.Opinion.ConditionsRequired", "errors.exams.readiness.opinion.conditionsRequired");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Readiness.Opinion.OperationConflict", "errors.exams.readiness.opinion.operationConflict");
}
