using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;

public static class ExamRegistrationSubmissionErrors
{
    public static readonly Error InvalidRegistration = Error.Validation("Exams.RegistrationSubmission.InvalidRegistration", "errors.exams.registrationSubmission.invalidRegistration");
    public static readonly Error InvalidFileRevision = Error.Validation("Exams.RegistrationSubmission.InvalidFileRevision", "errors.exams.registrationSubmission.invalidFileRevision");
    public static readonly Error InvalidProvider = Error.Validation("Exams.RegistrationSubmission.InvalidProvider", "errors.exams.registrationSubmission.invalidProvider");
    public static readonly Error InvalidOperation = Error.Validation("Exams.RegistrationSubmission.InvalidOperation", "errors.exams.registrationSubmission.invalidOperation");
    public static readonly Error FileNotReady = Error.Conflict("Exams.RegistrationSubmission.FileNotReady", "errors.exams.registrationSubmission.fileNotReady");
    public static readonly Error CandidateReferenceRequired = Error.Validation("Exams.RegistrationSubmission.CandidateReferenceRequired", "errors.exams.registrationSubmission.candidateReferenceRequired");
    public static readonly Error ProviderNotFound = Error.NotFound("Exams.RegistrationSubmission.ProviderNotFound", "errors.exams.registrationSubmission.providerNotFound");
    public static readonly Error ProviderCapabilityMissing = Error.Conflict("Exams.RegistrationSubmission.ProviderCapabilityMissing", "errors.exams.registrationSubmission.providerCapabilityMissing");
    public static readonly Error ProviderUnavailable = Error.Conflict("Exams.RegistrationSubmission.ProviderUnavailable", "errors.exams.registrationSubmission.providerUnavailable");
    public static readonly Error ProviderRejected = Error.Conflict("Exams.RegistrationSubmission.ProviderRejected", "errors.exams.registrationSubmission.providerRejected");
    public static readonly Error FileRevisionAlreadySubmitted = Error.Conflict("Exams.RegistrationSubmission.FileRevisionAlreadySubmitted", "errors.exams.registrationSubmission.fileRevisionAlreadySubmitted");
    public static readonly Error OperationConflict = Error.Conflict("Exams.RegistrationSubmission.OperationConflict", "errors.exams.registrationSubmission.operationConflict");
    public static readonly Error AlreadyFinalized = Error.Conflict("Exams.RegistrationSubmission.AlreadyFinalized", "errors.exams.registrationSubmission.alreadyFinalized");
    public static readonly Error NotFound = Error.NotFound("Exams.RegistrationSubmission.NotFound", "errors.exams.registrationSubmission.notFound");
}
