using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;

namespace DriveOS.Modules.RegulatoryIntegrations.Application.Administration;

public sealed record RegulatoryTrainingRecordSubmissionFilter(
    RegulatoryTrainingRecordSubmissionStatus? Status,
    string? CountryCode,
    string? ProviderCode,
    Guid? StudentId,
    Guid? TrainingPathId,
    Guid? SessionId,
    DateTimeOffset? CreatedFromUtc,
    DateTimeOffset? CreatedToUtc,
    int Page = 1,
    int PageSize = 50);

public sealed record RegulatoryTrainingRecordSubmissionListItem(
    Guid Id,
    Guid ProjectionId,
    Guid StudentId,
    Guid TrainingPathId,
    Guid SessionId,
    string CountryCode,
    string ProviderCode,
    RegulatoryTrainingRecordSubmissionStatus Status,
    int Revision,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    string? ExternalReference,
    string? LastErrorCode,
    bool HasIssues);

public sealed record RegulatoryTrainingRecordSubmissionRevision(
    Guid Id,
    int Revision,
    RegulatoryTrainingRecordSubmissionStatus Status,
    string PayloadHash,
    Guid? SupersedesSubmissionId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    string? ExternalReference,
    string? LastErrorCode,
    string? LastErrorDetail);

public sealed record RegulatoryTrainingRecordSubmissionDetail(
    Guid Id,
    Guid ProjectionId,
    int ProjectionSchemaVersion,
    Guid StudentId,
    Guid TrainingPathId,
    Guid SessionId,
    string CountryCode,
    string ProviderCode,
    RegulatoryTrainingRecordSubmissionStatus Status,
    int Revision,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    string? ExternalReference,
    string? LastErrorCode,
    string? LastErrorDetail,
    string IssuesJson,
    IReadOnlyList<RegulatoryTrainingRecordSubmissionRevision> Revisions);

public sealed record RegulatoryTrainingRecordSubmissionPage(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<RegulatoryTrainingRecordSubmissionListItem> Items);

public sealed record RegulatoryTrainingRecordSynchronizationSummary(
    int Total,
    int WaitingForData,
    int Pending,
    int Processing,
    int Accepted,
    int Rejected,
    int RetryPending,
    int Failed,
    int Superseded,
    DateTimeOffset? LastAcceptedAtUtc,
    DateTimeOffset? LastFailureAtUtc);

public sealed record StudentRegulatoryTrainingRecordOverview(
    Guid StudentId,
    Guid? TrainingPathId,
    string CountryCode,
    string ProviderCode,
    RegulatoryTrainingRecordSubmissionStatus? CurrentStatus,
    int TotalSubmissions,
    int Accepted,
    int WaitingForData,
    int Pending,
    int Rejected,
    int RetryPending,
    int Failed,
    DateTimeOffset? LastActivityAtUtc,
    DateTimeOffset? LastAcceptedAtUtc,
    string? LastErrorCode,
    string? LastErrorDetail,
    IReadOnlyList<string> CurrentIssues,
    IReadOnlyList<StudentRegulatoryTrainingRecordRecentSubmission> RecentSubmissions);

public sealed record StudentRegulatoryTrainingRecordRecentSubmission(
    Guid Id,
    Guid SessionId,
    RegulatoryTrainingRecordSubmissionStatus Status,
    int Revision,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    string? ExternalReference,
    string? LastErrorCode,
    bool HasIssues);
