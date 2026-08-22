namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

public interface IExamProviderErrorMapper
{
    ExamProviderMappedError Map(string providerCode, string? providerErrorCode, bool correctionRequested);
}

public sealed record ExamProviderMappedError(string Code, string MessageKey);
