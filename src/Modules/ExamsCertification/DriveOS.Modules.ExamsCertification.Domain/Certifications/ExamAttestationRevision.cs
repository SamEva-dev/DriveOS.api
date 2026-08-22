using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Certifications;

public sealed class ExamAttestationRevision
{
    private ExamAttestationRevision() { }

    internal ExamAttestationRevision(ExamAttestationRevisionId id, ExamAttestationId attestationId, int version,
        string templateCode, int templateVersion, DocumentId documentId, string documentSha256,
        string? publicVerificationTokenHash, UserId generatedByUserId, DateTimeOffset generatedAtUtc)
    {
        Id = id;
        AttestationId = attestationId;
        Version = version;
        TemplateCode = templateCode;
        TemplateVersion = templateVersion;
        DocumentId = documentId;
        DocumentSha256 = documentSha256;
        PublicVerificationTokenHash = publicVerificationTokenHash;
        GeneratedByUserId = generatedByUserId;
        GeneratedAtUtc = generatedAtUtc.ToUniversalTime();
    }

    public ExamAttestationRevisionId Id { get; private set; }
    public ExamAttestationId AttestationId { get; private set; }
    public int Version { get; private set; }
    public string TemplateCode { get; private set; } = string.Empty;
    public int TemplateVersion { get; private set; }
    public DocumentId DocumentId { get; private set; }
    public string DocumentSha256 { get; private set; } = string.Empty;
    public string? PublicVerificationTokenHash { get; private set; }
    public string? SignatureProcessReference { get; private set; }
    public string? SignatureEvidenceHash { get; private set; }
    public UserId? SignedByUserId { get; private set; }
    public DateTimeOffset? SignedAtUtc { get; private set; }
    public UserId GeneratedByUserId { get; private set; }
    public DateTimeOffset GeneratedAtUtc { get; private set; }

    internal Result RecordSignature(string signatureProcessReference, string signatureEvidenceHash, UserId signer, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(signatureProcessReference) || string.IsNullOrWhiteSpace(signatureEvidenceHash) || signer.IsEmpty)
            return Result.Failure(ExamAttestationErrors.InvalidSignature);
        SignatureProcessReference = signatureProcessReference.Trim();
        SignatureEvidenceHash = signatureEvidenceHash.Trim();
        SignedByUserId = signer;
        SignedAtUtc = now.ToUniversalTime();
        return Result.Success();
    }
}
