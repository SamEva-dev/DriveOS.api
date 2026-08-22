using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Submissions;

public sealed record SubmitExamRegistrationCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    Guid OperationId,
    UserId ActorUserId) : ICommand<ExamRegistrationSubmissionResponse>;

public sealed record RecordExamRegistrationOfficialResponseCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    ExamRegistrationSubmissionId SubmissionId,
    OfficialExamRegistrationOutcome Outcome,
    string? ExternalSubmissionId,
    string? ExternalRegistrationId,
    string? CandidateReference,
    string? ProviderResponseCode,
    string? ProviderResponseJson,
    string? ProviderErrorCode,
    UserId ActorUserId) : ICommand<ExamRegistrationSubmissionResponse>;

public sealed record RetryExamRegistrationSubmissionCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    ExamRegistrationSubmissionId SubmissionId,
    UserId ActorUserId) : ICommand<ExamRegistrationSubmissionResponse>;

public sealed record GetExamRegistrationSubmissionsQuery(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : IQuery<IReadOnlyList<ExamRegistrationSubmissionResponse>>;

public enum OfficialExamRegistrationOutcome
{
    Submitted = 1,
    Accepted = 2,
    Rejected = 3,
    CorrectionRequested = 4
}

public sealed record ExamRegistrationSubmissionResponse(
    Guid Id,
    Guid RegistrationId,
    Guid RegistrationFileId,
    Guid FileRevisionId,
    int FileVersion,
    int SubmissionVersion,
    string ProviderCode,
    string Status,
    string? ExternalSubmissionId,
    string? ExternalRegistrationId,
    string? CandidateReference,
    string? ProviderResponseCode,
    string? ErrorCode,
    string? ErrorMessageKey,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? RespondedAtUtc,
    DateTimeOffset CreatedAtUtc);
