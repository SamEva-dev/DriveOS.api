using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;

public readonly record struct RegulatoryTrainingRecordSubmissionId(Guid Value)
{
    public static RegulatoryTrainingRecordSubmissionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
}

public enum RegulatoryTrainingRecordSubmissionStatus
{
    WaitingForData = 1,
    Pending = 2,
    Processing = 3,
    Submitted = 4,
    Accepted = 5,
    Rejected = 6,
    RetryPending = 7,
    Failed = 8,
    Cancelled = 9,
    Superseded = 10
}

/// <summary>
/// Durable provider-independent submission snapshot. The normalized payload is frozen at creation
/// so retries never depend on mutable Student, Workforce, Pedagogy or Training Delivery data.
/// </summary>
public sealed class RegulatoryTrainingRecordSubmission : AggregateRoot<RegulatoryTrainingRecordSubmissionId>
{
    private RegulatoryTrainingRecordSubmission() { }

    private RegulatoryTrainingRecordSubmission(
        RegulatoryTrainingRecordSubmissionId id, Guid projectionId, int projectionSchemaVersion,
        OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, TrainingSessionId sessionId, string countryCode, string providerCode,
        string payloadJson, string payloadHash, string issuesJson, bool complete, DateTimeOffset createdAtUtc,
        int revision, RegulatoryTrainingRecordSubmissionId? supersedesSubmissionId) : base(id)
    {
        ProjectionId = projectionId;
        ProjectionSchemaVersion = projectionSchemaVersion;
        OrganizationId = organizationId;
        StudentId = studentId;
        TrainingPathId = trainingPathId;
        SessionId = sessionId;
        CountryCode = countryCode;
        ProviderCode = providerCode;
        PayloadJson = payloadJson;
        PayloadHash = payloadHash;
        IssuesJson = issuesJson;
        Status = complete ? RegulatoryTrainingRecordSubmissionStatus.Pending : RegulatoryTrainingRecordSubmissionStatus.WaitingForData;
        Revision = revision;
        SupersedesSubmissionId = supersedesSubmissionId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        NextAttemptAtUtc = complete ? CreatedAtUtc : null;
    }

    public Guid ProjectionId { get; private set; }
    public int ProjectionSchemaVersion { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public TrainingSessionId SessionId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string ProviderCode { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public string PayloadHash { get; private set; } = string.Empty;
    public string IssuesJson { get; private set; } = "[]";
    public int Revision { get; private set; }
    public RegulatoryTrainingRecordSubmissionId? SupersedesSubmissionId { get; private set; }
    public RegulatoryTrainingRecordSubmissionStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? AcknowledgedAtUtc { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorDetail { get; private set; }

    public static Result<RegulatoryTrainingRecordSubmission> Create(
        RegulatoryTrainingRecordSubmissionId id, Guid projectionId, int projectionSchemaVersion,
        OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, TrainingSessionId sessionId, string countryCode, string providerCode,
        string payloadJson, string payloadHash, string issuesJson, bool complete, DateTimeOffset createdAtUtc,
        int revision = 1, RegulatoryTrainingRecordSubmissionId? supersedesSubmissionId = null)
    {
        if (id.IsEmpty || projectionId == Guid.Empty || organizationId.IsEmpty || studentId.IsEmpty || trainingPathId.IsEmpty || sessionId.IsEmpty)
            return Result.Failure<RegulatoryTrainingRecordSubmission>(Error.Validation("RegulatoryIntegrations.Submission.InvalidIdentifier", "errors.regulatoryIntegrations.submission.invalidIdentifier"));
        if (projectionSchemaVersion <= 0 || revision <= 0 || string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(providerCode))
            return Result.Failure<RegulatoryTrainingRecordSubmission>(Error.Validation("RegulatoryIntegrations.Submission.InvalidProvider", "errors.regulatoryIntegrations.submission.invalidProvider"));
        if (string.IsNullOrWhiteSpace(payloadJson) || string.IsNullOrWhiteSpace(payloadHash))
            return Result.Failure<RegulatoryTrainingRecordSubmission>(Error.Validation("RegulatoryIntegrations.Submission.InvalidPayload", "errors.regulatoryIntegrations.submission.invalidPayload"));
        return Result.Success(new RegulatoryTrainingRecordSubmission(id, projectionId, projectionSchemaVersion, organizationId, studentId, trainingPathId, sessionId, countryCode.Trim().ToUpperInvariant(), providerCode.Trim(), payloadJson, payloadHash, string.IsNullOrWhiteSpace(issuesJson) ? "[]" : issuesJson, complete, createdAtUtc, revision, supersedesSubmissionId));
    }

    public bool HasPayloadHash(string payloadHash) =>
        string.Equals(PayloadHash, payloadHash, StringComparison.OrdinalIgnoreCase);

    public bool CanRefreshSnapshot => Status is
        RegulatoryTrainingRecordSubmissionStatus.WaitingForData or
        RegulatoryTrainingRecordSubmissionStatus.Pending or
        RegulatoryTrainingRecordSubmissionStatus.RetryPending or
        RegulatoryTrainingRecordSubmissionStatus.Rejected or
        RegulatoryTrainingRecordSubmissionStatus.Failed;

    public Result RefreshSnapshot(string payloadJson, string payloadHash, string issuesJson, bool complete, DateTimeOffset refreshedAtUtc)
    {
        if (!CanRefreshSnapshot)
            return InvalidTransition();
        if (string.IsNullOrWhiteSpace(payloadJson) || string.IsNullOrWhiteSpace(payloadHash))
            return Result.Failure(Error.Validation("RegulatoryIntegrations.Submission.InvalidPayload", "errors.regulatoryIntegrations.submission.invalidPayload"));

        PayloadJson = payloadJson;
        PayloadHash = payloadHash;
        IssuesJson = string.IsNullOrWhiteSpace(issuesJson) ? "[]" : issuesJson;
        Status = complete ? RegulatoryTrainingRecordSubmissionStatus.Pending : RegulatoryTrainingRecordSubmissionStatus.WaitingForData;
        NextAttemptAtUtc = complete ? refreshedAtUtc.ToUniversalTime() : null;
        LastErrorCode = null;
        LastErrorDetail = null;
        AcknowledgedAtUtc = null;
        return Result.Success();
    }

    public Result MarkSuperseded()
    {
        if (Status != RegulatoryTrainingRecordSubmissionStatus.Accepted)
            return InvalidTransition();
        Status = RegulatoryTrainingRecordSubmissionStatus.Superseded;
        NextAttemptAtUtc = null;
        return Result.Success();
    }

    public bool IsDue(DateTimeOffset nowUtc) =>
        Status is RegulatoryTrainingRecordSubmissionStatus.Pending or RegulatoryTrainingRecordSubmissionStatus.RetryPending
        || (Status == RegulatoryTrainingRecordSubmissionStatus.Processing
            && NextAttemptAtUtc is not null
            && NextAttemptAtUtc <= nowUtc.ToUniversalTime());

    public Result StartProcessing(DateTimeOffset attemptedAtUtc, DateTimeOffset leaseExpiresAtUtc)
    {
        DateTimeOffset attempted = attemptedAtUtc.ToUniversalTime();
        DateTimeOffset leaseExpires = leaseExpiresAtUtc.ToUniversalTime();

        if (!IsDue(attempted) || leaseExpires <= attempted)
            return Result.Failure(Error.Conflict("RegulatoryIntegrations.Submission.NotDispatchable", "errors.regulatoryIntegrations.submission.notDispatchable"));

        Status = RegulatoryTrainingRecordSubmissionStatus.Processing;
        AttemptCount++;
        LastAttemptAtUtc = attempted;
        NextAttemptAtUtc = leaseExpires;
        LastErrorCode = null;
        LastErrorDetail = null;
        return Result.Success();
    }

    public Result MarkAccepted(DateTimeOffset acknowledgedAtUtc, string? externalReference)
    {
        if (Status != RegulatoryTrainingRecordSubmissionStatus.Processing)
            return InvalidTransition();

        DateTimeOffset acknowledged = acknowledgedAtUtc.ToUniversalTime();
        Status = RegulatoryTrainingRecordSubmissionStatus.Accepted;
        SubmittedAtUtc ??= LastAttemptAtUtc ?? acknowledged;
        AcknowledgedAtUtc = acknowledged;
        NextAttemptAtUtc = null;
        ExternalReference = NormalizeNullable(externalReference) ?? ExternalReference;
        LastErrorCode = null;
        LastErrorDetail = null;
        return Result.Success();
    }

    public Result MarkRejected(DateTimeOffset acknowledgedAtUtc, string code, string? detail, string? externalReference)
    {
        if (Status != RegulatoryTrainingRecordSubmissionStatus.Processing)
            return InvalidTransition();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(Error.Validation("RegulatoryIntegrations.Submission.ErrorCodeRequired", "errors.regulatoryIntegrations.submission.errorCodeRequired"));

        DateTimeOffset acknowledged = acknowledgedAtUtc.ToUniversalTime();
        Status = RegulatoryTrainingRecordSubmissionStatus.Rejected;
        SubmittedAtUtc ??= LastAttemptAtUtc ?? acknowledged;
        AcknowledgedAtUtc = acknowledged;
        NextAttemptAtUtc = null;
        ExternalReference = NormalizeNullable(externalReference) ?? ExternalReference;
        LastErrorCode = code.Trim();
        LastErrorDetail = NormalizeNullable(detail);
        return Result.Success();
    }

    public Result ScheduleRetry(DateTimeOffset attemptedAtUtc, DateTimeOffset nextAttemptAtUtc, string code, string? detail)
    {
        if (Status != RegulatoryTrainingRecordSubmissionStatus.Processing)
            return InvalidTransition();
        if (string.IsNullOrWhiteSpace(code) || nextAttemptAtUtc.ToUniversalTime() <= attemptedAtUtc.ToUniversalTime())
            return Result.Failure(Error.Validation("RegulatoryIntegrations.Submission.InvalidRetry", "errors.regulatoryIntegrations.submission.invalidRetry"));

        Status = RegulatoryTrainingRecordSubmissionStatus.RetryPending;
        LastAttemptAtUtc = attemptedAtUtc.ToUniversalTime();
        NextAttemptAtUtc = nextAttemptAtUtc.ToUniversalTime();
        LastErrorCode = code.Trim();
        LastErrorDetail = NormalizeNullable(detail);
        return Result.Success();
    }

    public Result RequestManualRetry(DateTimeOffset requestedAtUtc)
    {
        if (Status is not (RegulatoryTrainingRecordSubmissionStatus.Rejected
            or RegulatoryTrainingRecordSubmissionStatus.Failed
            or RegulatoryTrainingRecordSubmissionStatus.RetryPending))
        {
            return InvalidTransition();
        }

        Status = RegulatoryTrainingRecordSubmissionStatus.Pending;
        NextAttemptAtUtc = requestedAtUtc.ToUniversalTime();
        LastErrorCode = null;
        LastErrorDetail = null;
        AcknowledgedAtUtc = null;
        return Result.Success();
    }

    public Result MarkFailed(DateTimeOffset attemptedAtUtc, string code, string? detail)
    {
        if (Status != RegulatoryTrainingRecordSubmissionStatus.Processing)
            return InvalidTransition();
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(Error.Validation("RegulatoryIntegrations.Submission.ErrorCodeRequired", "errors.regulatoryIntegrations.submission.errorCodeRequired"));

        Status = RegulatoryTrainingRecordSubmissionStatus.Failed;
        LastAttemptAtUtc = attemptedAtUtc.ToUniversalTime();
        NextAttemptAtUtc = null;
        LastErrorCode = code.Trim();
        LastErrorDetail = NormalizeNullable(detail);
        return Result.Success();
    }

    private static string? NormalizeNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result InvalidTransition() =>
        Result.Failure(Error.Conflict("RegulatoryIntegrations.Submission.InvalidTransition", "errors.regulatoryIntegrations.submission.invalidTransition"));

}
