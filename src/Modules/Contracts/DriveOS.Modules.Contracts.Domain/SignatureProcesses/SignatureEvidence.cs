using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.SignatureProcesses;

public sealed class SignatureEvidence
{
    private SignatureEvidence() { }

    private SignatureEvidence(
        SignatureEvidenceId id,
        SignatureProcessId signatureProcessId,
        TrainingContractSignatoryId signatoryId,
        PersonId personId,
        string documentSha256,
        string signatureMethod,
        string authenticationMethod,
        string provider,
        string providerSignatureReference,
        string? certificateReference,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset signedAtUtc,
        DateTimeOffset receivedAtUtc,
        UserId recordedByUserId)
    {
        Id = id;
        SignatureProcessId = signatureProcessId;
        SignatoryId = signatoryId;
        PersonId = personId;
        DocumentSha256 = documentSha256;
        SignatureMethod = signatureMethod;
        AuthenticationMethod = authenticationMethod;
        Provider = provider;
        ProviderSignatureReference = providerSignatureReference;
        CertificateReference = certificateReference;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        SignedAtUtc = signedAtUtc;
        ReceivedAtUtc = receivedAtUtc;
        RecordedByUserId = recordedByUserId;
    }

    public SignatureEvidenceId Id { get; private set; }
    public SignatureProcessId SignatureProcessId { get; private set; }
    public TrainingContractSignatoryId SignatoryId { get; private set; }
    public PersonId PersonId { get; private set; }
    public string DocumentSha256 { get; private set; } = string.Empty;
    public string SignatureMethod { get; private set; } = string.Empty;
    public string AuthenticationMethod { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string ProviderSignatureReference { get; private set; } = string.Empty;
    public string? CertificateReference { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset SignedAtUtc { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public UserId RecordedByUserId { get; private set; }

    internal static Result<SignatureEvidence> Create(
        SignatureEvidenceId id,
        SignatureProcessId signatureProcessId,
        TrainingContractSignatoryId signatoryId,
        PersonId personId,
        string documentSha256,
        string signatureMethod,
        string authenticationMethod,
        string provider,
        string providerSignatureReference,
        string? certificateReference,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset signedAtUtc,
        DateTimeOffset receivedAtUtc,
        UserId recordedByUserId)
    {
        string hash = Normalize(documentSha256).ToUpperInvariant();
        string method = Normalize(signatureMethod);
        string authentication = Normalize(authenticationMethod);
        string normalizedProvider = Normalize(provider);
        string providerReference = Normalize(providerSignatureReference);
        string? certificate = NormalizeNullable(certificateReference);
        string? ip = NormalizeNullable(ipAddress);
        string? agent = NormalizeNullable(userAgent);

        if (id.IsEmpty || signatureProcessId.IsEmpty || signatoryId.IsEmpty || personId.IsEmpty || recordedByUserId.IsEmpty)
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.InvalidEvidence);
        if (hash.Length != 64 || hash.Any(c => !Uri.IsHexDigit(c)))
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.InvalidEvidence);
        if (method.Length is < 2 or > 80 || authentication.Length is < 2 or > 120 || normalizedProvider.Length is < 2 or > 120 || providerReference.Length is < 2 or > 250)
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.InvalidEvidence);
        if (certificate is { Length: > 500 } || ip is { Length: > 64 } || agent is { Length: > 1000 })
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.InvalidEvidence);
        if (signedAtUtc == default || receivedAtUtc == default || signedAtUtc.ToUniversalTime() > receivedAtUtc.ToUniversalTime().AddMinutes(5))
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.InvalidEvidence);

        return Result.Success(new SignatureEvidence(
            id, signatureProcessId, signatoryId, personId, hash, method, authentication,
            normalizedProvider, providerReference, certificate, ip, agent,
            signedAtUtc.ToUniversalTime(), receivedAtUtc.ToUniversalTime(), recordedByUserId));
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
    private static string? NormalizeNullable(string? value)
    {
        string normalized = Normalize(value);
        return normalized.Length == 0 ? null : normalized;
    }
}
