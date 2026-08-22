using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness;

public static class ExamReadinessDecisionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "Exams.Readiness.Decision.InvalidIdentifier",
        "errors.exams.readiness.decision.invalidIdentifier");

    public static readonly Error InvalidOrganization = Error.Validation(
        "Exams.Readiness.Decision.InvalidOrganization",
        "errors.exams.readiness.decision.invalidOrganization");

    public static readonly Error InvalidStudent = Error.Validation(
        "Exams.Readiness.Decision.InvalidStudent",
        "errors.exams.readiness.decision.invalidStudent");

    public static readonly Error InvalidTrainingPath = Error.Validation(
        "Exams.Readiness.Decision.InvalidTrainingPath",
        "errors.exams.readiness.decision.invalidTrainingPath");

    public static readonly Error InvalidReviewer = Error.Validation(
        "Exams.Readiness.Decision.InvalidReviewer",
        "errors.exams.readiness.decision.invalidReviewer");

    public static readonly Error InvalidVersion = Error.Validation(
        "Exams.Readiness.Decision.InvalidVersion",
        "errors.exams.readiness.decision.invalidVersion");

    public static readonly Error InvalidStatus = Error.Validation(
        "Exams.Readiness.Decision.InvalidStatus",
        "errors.exams.readiness.decision.invalidStatus");

    public static readonly Error InvalidRationale = Error.Validation(
        "Exams.Readiness.Decision.InvalidRationale",
        "errors.exams.readiness.decision.invalidRationale");

    public static readonly Error ConditionsRequired = Error.Validation(
        "Exams.Readiness.Decision.ConditionsRequired",
        "errors.exams.readiness.decision.conditionsRequired");

    public static readonly Error ReadyWithBlockingCheck = Error.Conflict(
        "Exams.Readiness.Decision.ReadyWithBlockingCheck",
        "errors.exams.readiness.decision.readyWithBlockingCheck");

    public static readonly Error ReadyRequiresSatisfiedChecks = Error.Conflict(
        "Exams.Readiness.Decision.ReadyRequiresSatisfiedChecks",
        "errors.exams.readiness.decision.readyRequiresSatisfiedChecks");

    public static readonly Error AdministrativeBlockRequired = Error.Validation(
        "Exams.Readiness.Decision.AdministrativeBlockRequired",
        "errors.exams.readiness.decision.administrativeBlockRequired");

    public static readonly Error FinancialBlockRequired = Error.Validation(
        "Exams.Readiness.Decision.FinancialBlockRequired",
        "errors.exams.readiness.decision.financialBlockRequired");

    public static readonly Error RegulatoryBlockRequired = Error.Validation(
        "Exams.Readiness.Decision.RegulatoryBlockRequired",
        "errors.exams.readiness.decision.regulatoryBlockRequired");

    public static readonly Error AlreadySuperseded = Error.Conflict(
        "Exams.Readiness.Decision.AlreadySuperseded",
        "errors.exams.readiness.decision.alreadySuperseded");
}
