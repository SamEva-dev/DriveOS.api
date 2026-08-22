using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Success;

public enum ExamSuccessConsequenceKind
{
    PedagogicalCompletion = 1,
    StudentJourneyTransition = 2,
    ContractCompletion = 3,
    FinancialClosureReview = 4,
    CertificationEligibility = 5,
    SchedulingFollowUpReview = 6,
    SuccessCommunication = 7,
    AnalyticsMetrics = 8
}

public enum ExamSuccessConsequenceStatus
{
    Pending = 1,
    Processing = 2,
    Deferred = 3,
    Failed = 4,
    Processed = 5,
    DeadLetter = 6,
    Superseded = 7
}

public sealed record ExamSuccessSnapshot(
    OrganizationId OrganizationId,
    ExamResultId ResultId,
    ExamAttemptId AttemptId,
    ExamRegistrationId RegistrationId,
    PersonId StudentId,
    int AttemptNumber,
    int ResultRevision,
    string ExamType,
    string LicenseCategory,
    DateTimeOffset ExamCompletedAtUtc,
    DateTimeOffset ResultFinalizedAtUtc,
    UserId FinalizedByUserId);

public sealed record ExamSuccessConsequenceEnvelope(
    Guid Id,
    OrganizationId OrganizationId,
    ExamResultId ResultId,
    ExamSuccessConsequenceKind Kind,
    ExamSuccessConsequenceStatus Status,
    ExamSuccessSnapshot Snapshot,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    DateTimeOffset? SupersededAtUtc,
    string? LastErrorCode,
    string? LastErrorDetail);

public sealed record ExamSuccessConsequenceDispatchResult(
    bool IsProcessed,
    bool IsDeferred,
    bool IsPermanentFailure,
    string? Code = null,
    string? Detail = null)
{
    public static ExamSuccessConsequenceDispatchResult Processed() => new(true, false, false);
    public static ExamSuccessConsequenceDispatchResult Deferred(string code, string? detail = null) => new(false, true, false, code, detail);
    public static ExamSuccessConsequenceDispatchResult Retry(string code, string? detail = null) => new(false, false, false, code, detail);
    public static ExamSuccessConsequenceDispatchResult PermanentFailure(string code, string? detail = null) => new(false, false, true, code, detail);
}

public interface IExamSuccessConsequenceStore
{
    Task EnqueueAsync(ExamSuccessSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamSuccessConsequenceEnvelope>> ClaimDueAsync(int maxCount, DateTimeOffset now, TimeSpan processingLease, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamSuccessConsequenceEnvelope>> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid id, DateTimeOffset processedAtUtc, CancellationToken cancellationToken = default);
    Task MarkDeferredAsync(Guid id, string code, string? detail, DateTimeOffset attemptedAtUtc, DateTimeOffset nextAttemptAtUtc, CancellationToken cancellationToken = default);
    Task MarkFailedAsync(Guid id, string code, string? detail, bool permanent, DateTimeOffset attemptedAtUtc, DateTimeOffset? nextAttemptAtUtc, CancellationToken cancellationToken = default);
    Task SupersedeAsync(OrganizationId organizationId, ExamResultId resultId, int finalizedRevision, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task RequeueAsync(OrganizationId organizationId, ExamResultId resultId, DateTimeOffset now, CancellationToken cancellationToken = default);
}

public interface IExamSuccessConsequenceGateway
{
    Task<ExamSuccessConsequenceDispatchResult> DispatchAsync(ExamSuccessConsequenceEnvelope consequence, CancellationToken cancellationToken = default);
}

public sealed record ExamSuccessConsequenceResponse(
    Guid Id,
    string Kind,
    string Status,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    DateTimeOffset? SupersededAtUtc,
    string? LastErrorCode,
    string? LastErrorDetail);

public sealed record GetExamSuccessConsequencesQuery(OrganizationId OrganizationId, ExamResultId ResultId)
    : IQuery<IReadOnlyList<ExamSuccessConsequenceResponse>>;

public sealed record RequeueExamSuccessConsequencesCommand(OrganizationId OrganizationId, ExamResultId ResultId)
    : ICommand<IReadOnlyList<ExamSuccessConsequenceResponse>>;
