using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results;

/// <summary>Immutable evidence-bearing snapshot of a result version. Corrections append a revision; they never overwrite prior facts.</summary>
public sealed class ExamResultRevision
{
    private ExamResultRevision() { }

    internal ExamResultRevision(ExamResultRevisionId id, ExamResultId resultId, OrganizationId organizationId, int revisionNumber,
        ExamResultOutcome outcome, decimal? score, string? failureReasonCode, string? comments, ExamResultSourceKind sourceKind,
        string providerCode, string? externalResultId, DocumentId? evidenceDocumentId, DateTimeOffset receivedAtUtc,
        string? correctionReason, Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset createdAtUtc)
    {
        Id = id; ResultId = resultId; OrganizationId = organizationId; RevisionNumber = revisionNumber; Outcome = outcome; Score = score;
        FailureReasonCode = failureReasonCode; Comments = comments; SourceKind = sourceKind; ProviderCode = providerCode;
        ExternalResultId = externalResultId; EvidenceDocumentId = evidenceDocumentId; ReceivedAtUtc = receivedAtUtc.ToUniversalTime();
        CorrectionReason = correctionReason; OperationId = operationId; RequestFingerprint = requestFingerprint;
        ActorUserId = actorUserId; CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public ExamResultRevisionId Id { get; private set; }
    public ExamResultId ResultId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public int RevisionNumber { get; private set; }
    public ExamResultOutcome Outcome { get; private set; }
    public decimal? Score { get; private set; }
    public string? FailureReasonCode { get; private set; }
    public string? Comments { get; private set; }
    public ExamResultSourceKind SourceKind { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string? ExternalResultId { get; private set; }
    public DocumentId? EvidenceDocumentId { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public string? CorrectionReason { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
