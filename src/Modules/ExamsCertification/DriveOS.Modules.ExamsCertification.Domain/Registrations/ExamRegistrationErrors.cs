using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations;

public static class ExamRegistrationErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Exams.Registration.InvalidIdentifier", "errors.exams.registration.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Exams.Registration.InvalidOrganization", "errors.exams.registration.invalidOrganization");
    public static readonly Error InvalidStudent = Error.Validation("Exams.Registration.InvalidStudent", "errors.exams.registration.invalidStudent");
    public static readonly Error InvalidTrainingPath = Error.Validation("Exams.Registration.InvalidTrainingPath", "errors.exams.registration.invalidTrainingPath");
    public static readonly Error InvalidReadinessDecision = Error.Validation("Exams.Registration.InvalidReadinessDecision", "errors.exams.registration.invalidReadinessDecision");
    public static readonly Error InvalidPlace = Error.Validation("Exams.Registration.InvalidPlace", "errors.exams.registration.invalidPlace");
    public static readonly Error InvalidCenter = Error.Validation("Exams.Registration.InvalidCenter", "errors.exams.registration.invalidCenter");
    public static readonly Error InvalidExam = Error.Validation("Exams.Registration.InvalidExam", "errors.exams.registration.invalidExam");
    public static readonly Error InvalidPeriod = Error.Validation("Exams.Registration.InvalidPeriod", "errors.exams.registration.invalidPeriod");
    public static readonly Error InvalidOperation = Error.Validation("Exams.Registration.InvalidOperation", "errors.exams.registration.invalidOperation");
    public static readonly Error ReadinessNotEligible = Error.Conflict("Exams.Registration.ReadinessNotEligible", "errors.exams.registration.readinessNotEligible");
    public static readonly Error ReadinessChanged = Error.Conflict("Exams.Registration.ReadinessChanged", "errors.exams.registration.readinessChanged");
    public static readonly Error ActiveRegistrationAlreadyExists = Error.Conflict("Exams.Registration.ActiveAlreadyExists", "errors.exams.registration.activeAlreadyExists");
    public static readonly Error OperationConflict = Error.Conflict("Exams.Registration.OperationConflict", "errors.exams.registration.operationConflict");
    public static readonly Error NotFound = Error.NotFound("Exams.Registration.NotFound", "errors.exams.registration.notFound");
    public static readonly Error PlaceNotFound = Error.NotFound("Exams.Registration.PlaceNotFound", "errors.exams.registration.placeNotFound");
    public static readonly Error PlaceNotHeldByRequester = Error.Conflict("Exams.Registration.PlaceNotHeldByRequester", "errors.exams.registration.placeNotHeldByRequester");
    public static readonly Error CandidateReferenceRequired = Error.Validation("Exams.Registration.CandidateReferenceRequired", "errors.exams.registration.candidateReferenceRequired");
    public static readonly Error OfficialDataLocked = Error.Conflict("Exams.Registration.OfficialDataLocked", "errors.exams.registration.officialDataLocked");
    public static readonly Error InvalidSubmissionTransition = Error.Conflict("Exams.Registration.InvalidSubmissionTransition", "errors.exams.registration.invalidSubmissionTransition");
}
