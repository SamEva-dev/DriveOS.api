using DriveOS.Modules.ExamsCertification.Domain.Results.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Results;

/// <summary>
/// Authoritative DriveOS representation of the result of one concrete ExamAttempt. External systems remain the source of evidence;
/// this aggregate owns result lifecycle, verification, finalization and immutable correction history.
/// </summary>
public sealed class ExamResult : AggregateRoot<ExamResultId>, IAuditableEntity
{
    private readonly List<ExamResultRevision> _revisions = [];
    private ExamResult() { }

    private ExamResult(ExamResultId id, OrganizationId organizationId, ExamAttemptId attemptId, ExamRegistrationId registrationId,
        PersonId studentId, int attemptNumber, UserId actorUserId, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId; AttemptId = attemptId; RegistrationId = registrationId; StudentId = studentId;
        AttemptNumber = attemptNumber; Status = ExamResultStatus.Recorded; CreatedAtUtc = now.ToUniversalTime(); CreatedByUserId = actorUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamAttemptId AttemptId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public int CurrentRevision { get; private set; }
    public ExamResultOutcome Outcome { get; private set; }
    public decimal? Score { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public string? Comments { get; private set; }
    public ExamResultSourceKind SourceKind { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string? ExternalResultId { get; private set; }
    public DocumentId? EvidenceDocumentId { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public ExamResultStatus Status { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public UserId? VerifiedByUserId { get; private set; }
    public string? VerificationReference { get; private set; }
    public DateTimeOffset? FinalizedAtUtc { get; private set; }
    public UserId? FinalizedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamResultRevision> Revisions => _revisions.AsReadOnly();

    public static Result<ExamResult> Create(OrganizationId organizationId, ExamAttemptId attemptId, ExamRegistrationId registrationId,
        PersonId studentId, int attemptNumber, ExamResultOutcome outcome, decimal? score, string? failureReasonCode, string? comments,
        ExamResultSourceKind sourceKind, string providerCode, string? externalResultId, DocumentId? evidenceDocumentId,
        DateTimeOffset receivedAtUtc, Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset now)
    {
        if (organizationId.IsEmpty || attemptId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty || actorUserId.IsEmpty || attemptNumber <= 0)
            return Result.Failure<ExamResult>(ExamResultErrors.InvalidIdentifier);
        Result validation = ValidatePayload(score, sourceKind, providerCode, operationId, requestFingerprint);
        if (validation.IsFailure) return Result.Failure<ExamResult>(validation.Error);

        var result = new ExamResult(ExamResultId.New(), organizationId, attemptId, registrationId, studentId, attemptNumber, actorUserId, now);
        result.AppendRevision(outcome, score, failureReasonCode, comments, sourceKind, providerCode, externalResultId, evidenceDocumentId,
            receivedAtUtc, null, operationId, requestFingerprint, actorUserId, now);
        result.RaiseDomainEvent(new ExamResultRecordedDomainEvent(result.Id, attemptId, organizationId, outcome, result.CurrentRevision));
        return Result.Success(result);
    }

    public Result Verify(string verificationReference, UserId actorUserId, DateTimeOffset now)
    {
        if (Status != ExamResultStatus.Recorded) return Result.Failure(ExamResultErrors.InvalidTransition);
        if (string.IsNullOrWhiteSpace(verificationReference) && EvidenceDocumentId is null && string.IsNullOrWhiteSpace(ExternalResultId))
            return Result.Failure(ExamResultErrors.VerificationEvidenceRequired);
        Status = ExamResultStatus.Verified; VerificationReference = Normalize(verificationReference); VerifiedAtUtc = now.ToUniversalTime();
        VerifiedByUserId = actorUserId; Touch(actorUserId, now);
        RaiseDomainEvent(new ExamResultVerifiedDomainEvent(Id, AttemptId, OrganizationId, CurrentRevision));
        return Result.Success();
    }

    public Result Finalize(UserId actorUserId, DateTimeOffset now)
    {
        if (Status != ExamResultStatus.Verified) return Result.Failure(ExamResultErrors.InvalidTransition);
        Status = ExamResultStatus.Finalized; FinalizedAtUtc = now.ToUniversalTime(); FinalizedByUserId = actorUserId; Touch(actorUserId, now);
        RaiseDomainEvent(new ExamResultFinalizedDomainEvent(Id, AttemptId, OrganizationId, Outcome, CurrentRevision));
        if (Outcome == ExamResultOutcome.Passed)
            RaiseDomainEvent(new ExamPassedDomainEvent(Id, AttemptId, OrganizationId, StudentId, AttemptNumber));
        else if (Outcome == ExamResultOutcome.Failed)
            RaiseDomainEvent(new ExamFailedDomainEvent(Id, AttemptId, OrganizationId, StudentId, AttemptNumber));
        return Result.Success();
    }

    public Result Correct(ExamResultOutcome outcome, decimal? score, string? failureReasonCode, string? comments,
        ExamResultSourceKind sourceKind, string providerCode, string? externalResultId, DocumentId? evidenceDocumentId,
        DateTimeOffset receivedAtUtc, string correctionReason, Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(correctionReason)) return Result.Failure(ExamResultErrors.CorrectionReasonRequired);
        Result validation = ValidatePayload(score, sourceKind, providerCode, operationId, requestFingerprint);
        if (validation.IsFailure) return validation;
        ExamResultRevision? existing = _revisions.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return string.Equals(existing.RequestFingerprint, requestFingerprint, StringComparison.Ordinal)
                ? Result.Success() : Result.Failure(ExamResultErrors.OperationConflict);

        int previous = CurrentRevision;
        ExamResultOutcome previousOutcome = Outcome;
        bool supersedesFinalization = Status == ExamResultStatus.Finalized;
        AppendRevision(outcome, score, failureReasonCode, comments, sourceKind, providerCode, externalResultId, evidenceDocumentId,
            receivedAtUtc, correctionReason.Trim(), operationId, requestFingerprint, actorUserId, now);
        Status = ExamResultStatus.Recorded; VerifiedAtUtc = null; VerifiedByUserId = null; VerificationReference = null;
        FinalizedAtUtc = null; FinalizedByUserId = null; Touch(actorUserId, now);
        if (supersedesFinalization)
            RaiseDomainEvent(new ExamResultFinalizationSupersededDomainEvent(Id, AttemptId, OrganizationId, previousOutcome, previous));
        RaiseDomainEvent(new ExamResultCorrectedDomainEvent(Id, AttemptId, OrganizationId, previous, CurrentRevision, Outcome));
        return Result.Success();
    }

    public bool MatchesOperation(Guid operationId, string fingerprint) =>
        _revisions.Any(x => x.OperationId == operationId && string.Equals(x.RequestFingerprint, fingerprint, StringComparison.Ordinal));

    private void AppendRevision(ExamResultOutcome outcome, decimal? score, string? failureReasonCode, string? comments,
        ExamResultSourceKind sourceKind, string providerCode, string? externalResultId, DocumentId? evidenceDocumentId,
        DateTimeOffset receivedAtUtc, string? correctionReason, Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset now)
    {
        CurrentRevision++;
        Outcome = outcome; Score = score; FailureReasonCode = Normalize(failureReasonCode); Comments = Normalize(comments); SourceKind = sourceKind;
        ProviderCode = providerCode.Trim(); ExternalResultId = Normalize(externalResultId); EvidenceDocumentId = evidenceDocumentId;
        ReceivedAtUtc = receivedAtUtc.ToUniversalTime();
        _revisions.Add(new ExamResultRevision(ExamResultRevisionId.New(), Id, OrganizationId, CurrentRevision, outcome, score,
            FailureReasonCode, Comments, sourceKind, ProviderCode, ExternalResultId, evidenceDocumentId, ReceivedAtUtc,
            correctionReason, operationId, requestFingerprint, actorUserId, now));
    }

    private static Result ValidatePayload(decimal? score, ExamResultSourceKind sourceKind, string providerCode, Guid operationId, string fingerprint)
    {
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(fingerprint)) return Result.Failure(ExamResultErrors.InvalidOperation);
        if (score is < 0) return Result.Failure(ExamResultErrors.InvalidScore);
        if (!Enum.IsDefined(sourceKind) || string.IsNullOrWhiteSpace(providerCode)) return Result.Failure(ExamResultErrors.InvalidSource);
        return Result.Success();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private void Touch(UserId actorUserId, DateTimeOffset now) => SetModifiedAudit(now, actorUserId);
}
