using DriveOS.Modules.ExamsCertification.Domain.Certifications.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Certifications;

/// <summary>
/// Exam-owned issuance record for an exam attestation. Files and signature processes are external capabilities;
/// this aggregate owns exam provenance, document revisions, delivery, expiration, supersession and revocation semantics.
/// </summary>
public sealed class ExamAttestation : AggregateRoot<ExamAttestationId>, IAuditableEntity
{
    private readonly List<ExamAttestationRevision> _revisions = [];
    private ExamAttestation() { }

    private ExamAttestation(ExamAttestationId id, OrganizationId organizationId, ExamResultId resultId, int resultRevision,
        ExamAttemptId attemptId, ExamRegistrationId registrationId, PersonId studentId, int attemptNumber,
        ExamAttestationType type, string reference, ExamAttestationId? supersedesAttestationId,
        DateTimeOffset? expiresAtUtc, Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        ExamResultId = resultId;
        ResultRevision = resultRevision;
        ExamAttemptId = attemptId;
        ExamRegistrationId = registrationId;
        StudentId = studentId;
        AttemptNumber = attemptNumber;
        Type = type;
        Reference = reference;
        SupersedesAttestationId = supersedesAttestationId;
        ExpiresAtUtc = expiresAtUtc?.ToUniversalTime();
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        Status = ExamAttestationStatus.Generated;
        IssuedAtUtc = now.ToUniversalTime();
        IssuedByUserId = actorUserId;
        CreatedAtUtc = IssuedAtUtc;
        CreatedByUserId = actorUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamResultId ExamResultId { get; private set; }
    public int ResultRevision { get; private set; }
    public ExamAttemptId ExamAttemptId { get; private set; }
    public ExamRegistrationId ExamRegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public ExamAttestationType Type { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public int CurrentVersion { get; private set; }
    public ExamAttestationId? SupersedesAttestationId { get; private set; }
    public ExamAttestationStatus Status { get; private set; }
    public DateTimeOffset IssuedAtUtc { get; private set; }
    public UserId IssuedByUserId { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }
    public UserId? DeliveredByUserId { get; private set; }
    public ExamAttestationDeliveryChannel? DeliveryChannel { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public UserId? RevokedByUserId { get; private set; }
    public string? RevocationReasonCode { get; private set; }
    public string? RevocationNotes { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public IReadOnlyCollection<ExamAttestationRevision> Revisions => _revisions.AsReadOnly();
    public ExamAttestationRevision CurrentRevision => _revisions.Single(x => x.Version == CurrentVersion);

    public static Result<ExamAttestation> Issue(OrganizationId organizationId, ExamResultId resultId, int resultRevision,
        ExamAttemptId attemptId, ExamRegistrationId registrationId, PersonId studentId, int attemptNumber, ExamAttestationType type,
        string reference, ExamAttestationId? supersedesAttestationId, string templateCode, int templateVersion, DocumentId documentId,
        string documentSha256, string? publicVerificationTokenHash, DateTimeOffset? expiresAtUtc,
        Guid operationId, string requestFingerprint, UserId actorUserId, DateTimeOffset now)
    {
        if (organizationId.IsEmpty || resultId.IsEmpty || attemptId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty || actorUserId.IsEmpty || attemptNumber <= 0 || resultRevision <= 0)
            return Result.Failure<ExamAttestation>(ExamAttestationErrors.InvalidIdentifier);
        if (!Enum.IsDefined(type)) return Result.Failure<ExamAttestation>(ExamAttestationErrors.InvalidType);
        if (documentId.IsEmpty || string.IsNullOrWhiteSpace(reference) || string.IsNullOrWhiteSpace(templateCode) || templateVersion <= 0 || string.IsNullOrWhiteSpace(documentSha256))
            return Result.Failure<ExamAttestation>(ExamAttestationErrors.InvalidDocument);
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint))
            return Result.Failure<ExamAttestation>(ExamAttestationErrors.InvalidDocument);
        if (expiresAtUtc.HasValue && expiresAtUtc.Value.ToUniversalTime() <= now.ToUniversalTime())
            return Result.Failure<ExamAttestation>(ExamAttestationErrors.InvalidDocument);

        var x = new ExamAttestation(ExamAttestationId.New(), organizationId, resultId, resultRevision, attemptId,
            registrationId, studentId, attemptNumber, type, reference.Trim(), supersedesAttestationId, expiresAtUtc,
            operationId, requestFingerprint, actorUserId, now);
        x.AddRevision(templateCode, templateVersion, documentId, documentSha256, publicVerificationTokenHash, actorUserId, now);
        x.RaiseDomainEvent(new ExamAttestationIssuedDomainEvent(x.Id, organizationId, resultId, resultRevision, type, documentId));
        return Result.Success(x);
    }

    public Result CorrectDocument(string templateCode, int templateVersion, DocumentId documentId, string documentSha256,
        string? publicVerificationTokenHash, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamAttestationStatus.Revoked or ExamAttestationStatus.Superseded or ExamAttestationStatus.Expired)
            return Result.Failure(ExamAttestationErrors.NotActive);
        if (documentId.IsEmpty || string.IsNullOrWhiteSpace(templateCode) || templateVersion <= 0 || string.IsNullOrWhiteSpace(documentSha256))
            return Result.Failure(ExamAttestationErrors.InvalidDocument);
        int previous = CurrentVersion;
        AddRevision(templateCode, templateVersion, documentId, documentSha256, publicVerificationTokenHash, actorUserId, now);
        Status = ExamAttestationStatus.Generated;
        DeliveredAtUtc = null;
        DeliveredByUserId = null;
        DeliveryChannel = null;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamAttestationDocumentCorrectedDomainEvent(Id, OrganizationId, previous, CurrentVersion, documentId));
        return Result.Success();
    }

    public Result RecordSignature(string signatureProcessReference, string signatureEvidenceHash, UserId actorUserId, DateTimeOffset now)
    {
        if (Status is ExamAttestationStatus.Revoked or ExamAttestationStatus.Superseded or ExamAttestationStatus.Expired)
            return Result.Failure(ExamAttestationErrors.NotActive);
        Result result = CurrentRevision.RecordSignature(signatureProcessReference, signatureEvidenceHash, actorUserId, now);
        if (result.IsFailure) return result;
        Status = ExamAttestationStatus.Signed;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamAttestationSignedDomainEvent(Id, OrganizationId, CurrentVersion, signatureProcessReference.Trim()));
        return Result.Success();
    }

    public Result MarkDelivered(ExamAttestationDeliveryChannel channel, UserId actorUserId, DateTimeOffset now)
    {
        if (!Enum.IsDefined(channel)) return Result.Failure(ExamAttestationErrors.InvalidDelivery);
        if (Status is ExamAttestationStatus.Revoked or ExamAttestationStatus.Superseded or ExamAttestationStatus.Expired)
            return Result.Failure(ExamAttestationErrors.NotActive);
        DeliveredAtUtc = now.ToUniversalTime();
        DeliveredByUserId = actorUserId;
        DeliveryChannel = channel;
        Status = ExamAttestationStatus.Delivered;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamAttestationDeliveredDomainEvent(Id, OrganizationId, CurrentVersion, channel));
        return Result.Success();
    }

    public Result Revoke(string reasonCode, string? notes, UserId actorUserId, DateTimeOffset now)
    {
        if (Status == ExamAttestationStatus.Revoked) return Result.Failure(ExamAttestationErrors.AlreadyRevoked);
        if (Status == ExamAttestationStatus.Superseded) return Result.Failure(ExamAttestationErrors.NotActive);
        if (string.IsNullOrWhiteSpace(reasonCode)) return Result.Failure(ExamAttestationErrors.RevocationReasonRequired);
        Status = ExamAttestationStatus.Revoked;
        RevocationReasonCode = reasonCode.Trim();
        RevocationNotes = Normalize(notes);
        RevokedAtUtc = now.ToUniversalTime();
        RevokedByUserId = actorUserId;
        Touch(actorUserId, now);
        RaiseDomainEvent(new ExamAttestationRevokedDomainEvent(Id, OrganizationId, ExamResultId, RevocationReasonCode));
        return Result.Success();
    }

    public void RefreshExpiration(DateTimeOffset now)
    {
        if (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= now.ToUniversalTime() && Status is not (ExamAttestationStatus.Revoked or ExamAttestationStatus.Superseded))
            Status = ExamAttestationStatus.Expired;
    }

    public void Supersede(DateTimeOffset now)
    {
        if (Status is ExamAttestationStatus.Revoked or ExamAttestationStatus.Superseded) return;
        Status = ExamAttestationStatus.Superseded;
        SupersededAtUtc = now.ToUniversalTime();
        RaiseDomainEvent(new ExamAttestationSupersededDomainEvent(Id, OrganizationId, ExamResultId, ResultRevision));
    }

    public bool IsPubliclyValid(DateTimeOffset now)
    {
        RefreshExpiration(now);
        return Status is ExamAttestationStatus.Generated or ExamAttestationStatus.Signed or ExamAttestationStatus.Delivered;
    }

    public bool MatchesOperation(Guid operationId, string fingerprint) => OperationId == operationId && string.Equals(RequestFingerprint, fingerprint, StringComparison.Ordinal);

    private void AddRevision(string templateCode, int templateVersion, DocumentId documentId, string documentSha256,
        string? publicVerificationTokenHash, UserId actorUserId, DateTimeOffset now)
    {
        CurrentVersion++;
        _revisions.Add(new ExamAttestationRevision(ExamAttestationRevisionId.New(), Id, CurrentVersion, templateCode.Trim(), templateVersion,
            documentId, documentSha256.Trim().ToLowerInvariant(), Normalize(publicVerificationTokenHash), actorUserId, now));
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
