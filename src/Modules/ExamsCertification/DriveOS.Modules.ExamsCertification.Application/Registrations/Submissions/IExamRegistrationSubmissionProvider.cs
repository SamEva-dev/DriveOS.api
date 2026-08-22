using DriveOS.Modules.ExamsCertification.Application.Providers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

/// <summary>
/// Anti-corruption boundary used to submit a versioned DriveOS examination dossier to an authorized external system.
/// Implementations must not leak provider-specific DTOs into BC-11.
/// </summary>
public interface IExamRegistrationSubmissionProvider
{
    ExamPlaceProviderDescriptor Descriptor { get; }

    Task<ExternalExamRegistrationSubmissionResult> SubmitAsync(
        ExternalExamRegistrationSubmissionRequest request,
        CancellationToken cancellationToken = default);
}

public interface IExamRegistrationSubmissionProviderResolver
{
    IExamRegistrationSubmissionProvider? Resolve(string providerCode);
}

public sealed record ExternalExamRegistrationSubmissionRequest(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    string? ExternalPlaceId,
    string? CandidateReference,
    string PayloadJson,
    string IdempotencyKey);

public sealed record ExternalExamRegistrationSubmissionResult(
    ExternalExamRegistrationSubmissionOutcome Outcome,
    string? ExternalSubmissionId = null,
    string? ExternalRegistrationId = null,
    string? CandidateReference = null,
    string? ProviderResponseCode = null,
    string? ProviderResponseJson = null,
    string? ProviderErrorCode = null);

public enum ExternalExamRegistrationSubmissionOutcome
{
    Submitted = 1,
    Accepted = 2,
    Rejected = 3,
    CorrectionRequested = 4,
    AwaitingManualSubmission = 5
}
