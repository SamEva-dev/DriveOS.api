using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Assessments;

public static class AssessmentSessionErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "Crm.Assessments.Session.InvalidIdentifier",
        "errors.crm.assessments.session.invalidIdentifier"
    );
    public static readonly Error AppointmentNotFound = Error.NotFound(
        "Crm.Assessments.AppointmentNotFound",
        "errors.crm.assessments.appointmentNotFound"
    );
    public static readonly Error AppointmentNotStartable = Error.Conflict(
        "Crm.Assessments.AppointmentNotStartable",
        "errors.crm.assessments.appointmentNotStartable"
    );
    public static readonly Error AlreadyStarted = Error.Conflict(
        "Crm.Assessments.Session.AlreadyStarted",
        "errors.crm.assessments.session.alreadyStarted"
    );
    public static readonly Error NotFound = Error.NotFound(
        "Crm.Assessments.Session.NotFound",
        "errors.crm.assessments.session.notFound"
    );
    public static readonly Error AlreadySubmitted = Error.Conflict(
        "Crm.Assessments.Session.AlreadySubmitted",
        "errors.crm.assessments.session.alreadySubmitted"
    );
    public static readonly Error InvalidQuestionnaire = Error.Validation(
        "Crm.Assessments.Session.InvalidQuestionnaire",
        "errors.crm.assessments.session.invalidQuestionnaire"
    );
    public static readonly Error InvalidAnswers = Error.Validation(
        "Crm.Assessments.Session.InvalidAnswers",
        "errors.crm.assessments.session.invalidAnswers"
    );
    public static readonly Error NotesTooLong = Error.Validation(
        "Crm.Assessments.Session.NotesTooLong",
        "errors.crm.assessments.session.notesTooLong"
    );
    public static readonly Error SubmissionRequiresAnswers = Error.Validation(
        "Crm.Assessments.Session.SubmissionRequiresAnswers",
        "errors.crm.assessments.session.submissionRequiresAnswers"
    );
    public static readonly Error ResultRequiresSubmittedAssessment = Error.Conflict(
        "Crm.Assessments.Result.RequiresSubmittedAssessment",
        "errors.crm.assessments.result.requiresSubmittedAssessment"
    );
    public static readonly Error InvalidResult = Error.Validation(
        "Crm.Assessments.Result.Invalid",
        "errors.crm.assessments.result.invalid"
    );
    public static readonly Error ResultNotReady = Error.Conflict(
        "Crm.Assessments.Result.NotReady",
        "errors.crm.assessments.result.notReady"
    );
    public static readonly Error InvalidCorrectionReason = Error.Validation(
        "Crm.Assessments.Result.InvalidCorrectionReason",
        "errors.crm.assessments.result.invalidCorrectionReason"
    );
    public static readonly Error ValidatedResultIsImmutable = Error.Conflict(
        "Crm.Assessments.Result.ValidatedIsImmutable",
        "errors.crm.assessments.result.validatedIsImmutable"
    );
    public static readonly Error ResultMustBeValidated = Error.Conflict(
        "Crm.Assessments.Result.MustBeValidated",
        "errors.crm.assessments.result.mustBeValidated"
    );
    public static readonly Error RevisionConflict = Error.Conflict(
        "Crm.Assessments.Session.RevisionConflict",
        "errors.crm.assessments.session.revisionConflict"
    );
}
