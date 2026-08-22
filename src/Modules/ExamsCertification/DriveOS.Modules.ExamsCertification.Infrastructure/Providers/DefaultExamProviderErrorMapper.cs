using DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Providers;

internal sealed class DefaultExamProviderErrorMapper : IExamProviderErrorMapper
{
    public ExamProviderMappedError Map(string providerCode, string? providerErrorCode, bool correctionRequested)
    {
        // Provider-specific mappings are intentionally added only from authorized specifications.
        // Raw external codes remain persisted in ProviderResponseJson/ProviderResponseCode for audit.
        return correctionRequested
            ? new ExamProviderMappedError("Exams.RegistrationSubmission.CorrectionRequested", "errors.exams.registrationSubmission.correctionRequested")
            : new ExamProviderMappedError("Exams.RegistrationSubmission.ProviderRejected", "errors.exams.registrationSubmission.providerRejected");
    }
}
