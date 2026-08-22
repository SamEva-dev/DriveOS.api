using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Results;

public sealed record ExamResultRevisionResponse(int Revision, string Outcome, decimal? Score, string? FailureReasonCode, string? Comments,
    string SourceKind, string ProviderCode, string? ExternalResultId, Guid? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc,
    string? CorrectionReason, Guid OperationId, Guid ActorUserId, DateTimeOffset CreatedAtUtc);

public sealed record ExamResultResponse(Guid Id, Guid AttemptId, Guid RegistrationId, Guid StudentId, int AttemptNumber,
    int CurrentRevision, string Outcome, decimal? Score, string? FailureReasonCode, string? Comments, string SourceKind,
    string ProviderCode, string? ExternalResultId, Guid? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc, string Status,
    DateTimeOffset? VerifiedAtUtc, Guid? VerifiedByUserId, string? VerificationReference,
    DateTimeOffset? FinalizedAtUtc, Guid? FinalizedByUserId, IReadOnlyList<ExamResultRevisionResponse> Revisions);

public sealed record RecordExamResultCommand(OrganizationId OrganizationId, ExamAttemptId AttemptId, ExamResultOutcomeInput Outcome,
    decimal? Score, string? FailureReasonCode, string? Comments, string SourceKind, string ProviderCode, string? ExternalResultId,
    DocumentId? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc, Guid OperationId, UserId ActorUserId) : ICommand<ExamResultResponse>;

public sealed record VerifyExamResultCommand(OrganizationId OrganizationId, ExamResultId ResultId, string VerificationReference, UserId ActorUserId)
    : ICommand<ExamResultResponse>;

public sealed record FinalizeExamResultCommand(OrganizationId OrganizationId, ExamResultId ResultId, UserId ActorUserId)
    : ICommand<ExamResultResponse>;

public sealed record CorrectExamResultCommand(OrganizationId OrganizationId, ExamResultId ResultId, ExamResultOutcomeInput Outcome,
    decimal? Score, string? FailureReasonCode, string? Comments, string SourceKind, string ProviderCode, string? ExternalResultId,
    DocumentId? EvidenceDocumentId, DateTimeOffset ReceivedAtUtc, string CorrectionReason, Guid OperationId, UserId ActorUserId)
    : ICommand<ExamResultResponse>;

public sealed record GetExamResultByAttemptQuery(OrganizationId OrganizationId, ExamAttemptId AttemptId) : IQuery<ExamResultResponse>;
public sealed record GetExamResultQuery(OrganizationId OrganizationId, ExamResultId ResultId) : IQuery<ExamResultResponse>;
public sealed record GetStudentExamResultsQuery(OrganizationId OrganizationId, PersonId StudentId) : IQuery<IReadOnlyList<ExamResultResponse>>;

public enum ExamResultOutcomeInput { Passed = 1, Failed = 2, Cancelled = 3, Invalidated = 4, NotEvaluated = 5 }
