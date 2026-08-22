using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.File;

public static class ExamRegistrationFileErrors
{
    public static readonly Error InvalidRegistration = Error.Validation("Exams.RegistrationFile.InvalidRegistration", "errors.exams.registrationFile.invalidRegistration");
    public static readonly Error InvalidSnapshot = Error.Validation("Exams.RegistrationFile.InvalidSnapshot", "errors.exams.registrationFile.invalidSnapshot");
    public static readonly Error CandidateReferenceRequired = Error.Validation("Exams.RegistrationFile.CandidateReferenceRequired", "errors.exams.registrationFile.candidateReferenceRequired");
    public static readonly Error NotReady = Error.Conflict("Exams.RegistrationFile.NotReady", "errors.exams.registrationFile.notReady");
    public static readonly Error RevisionLocked = Error.Conflict("Exams.RegistrationFile.RevisionLocked", "errors.exams.registrationFile.revisionLocked");
    public static readonly Error InvalidSubmissionTransition = Error.Conflict("Exams.RegistrationFile.InvalidSubmissionTransition", "errors.exams.registrationFile.invalidSubmissionTransition");
    public static readonly Error NotFound = Error.NotFound("Exams.RegistrationFile.NotFound", "errors.exams.registrationFile.notFound");
}
