using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.InstructorRegulatoryCredentials;

/// <summary>
/// Temporary pre-Workforce aggregate describing a regulatory credential attached to an instructor
/// within an organization. It intentionally does not model employment, job position or HR state.
/// BC-12 will become the authoritative professional-domain owner while preserving this integration contract.
/// </summary>
public sealed class InstructorRegulatoryCredential : AggregateRoot<InstructorRegulatoryCredentialId>
{
    private InstructorRegulatoryCredential() { }

    private InstructorRegulatoryCredential(
        InstructorRegulatoryCredentialId id,
        OrganizationId organizationId,
        UserId instructorUserId,
        string countryCode,
        string credentialType,
        string identifier,
        string issuingAuthority,
        string? jurisdictionCode,
        DateOnly? issuedOn,
        DateOnly? expiresOn,
        InstructorRegulatoryCredentialSource source,
        UserId actorUserId,
        DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        InstructorUserId = instructorUserId;
        CountryCode = countryCode;
        CredentialType = credentialType;
        Identifier = identifier;
        IssuingAuthority = issuingAuthority;
        JurisdictionCode = jurisdictionCode;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        Source = source;
        Status = InstructorRegulatoryCredentialStatus.Declared;
        DeclaredByUserId = actorUserId;
        DeclaredAtUtc = now;
    }

    public OrganizationId OrganizationId { get; private set; }
    public UserId InstructorUserId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string CredentialType { get; private set; } = string.Empty;
    public string Identifier { get; private set; } = string.Empty;
    public string IssuingAuthority { get; private set; } = string.Empty;
    public string? JurisdictionCode { get; private set; }
    public DateOnly? IssuedOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public InstructorRegulatoryCredentialSource Source { get; private set; }
    public InstructorRegulatoryCredentialStatus Status { get; private set; }
    public DateTimeOffset DeclaredAtUtc { get; private set; }
    public UserId DeclaredByUserId { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public UserId? VerifiedByUserId { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public InstructorRegulatoryCredentialId? SupersededById { get; private set; }

    public bool IsCurrent => Status is InstructorRegulatoryCredentialStatus.Declared or InstructorRegulatoryCredentialStatus.Verified;

    public static Result<InstructorRegulatoryCredential> Declare(
        OrganizationId organizationId,
        UserId instructorUserId,
        string countryCode,
        string credentialType,
        string identifier,
        string issuingAuthority,
        string? jurisdictionCode,
        DateOnly? issuedOn,
        DateOnly? expiresOn,
        InstructorRegulatoryCredentialSource source,
        UserId actorUserId,
        DateTimeOffset now)
    {
        string country = NormalizeToken(countryCode);
        string type = NormalizeToken(credentialType);
        string number = NormalizeIdentifier(identifier);
        string authority = (issuingAuthority ?? string.Empty).Trim();
        string? jurisdiction = NormalizeOptionalToken(jurisdictionCode);

        if (organizationId.IsEmpty || instructorUserId.IsEmpty || actorUserId.IsEmpty)
            return Result.Failure<InstructorRegulatoryCredential>(InstructorRegulatoryCredentialErrors.InvalidOwner);
        if (country.Length != 2 || type.Length is < 2 or > 64 || number.Length is < 1 or > 120)
            return Result.Failure<InstructorRegulatoryCredential>(InstructorRegulatoryCredentialErrors.InvalidCredential);
        if (authority.Length is < 2 or > 160)
            return Result.Failure<InstructorRegulatoryCredential>(InstructorRegulatoryCredentialErrors.InvalidIssuingAuthority);
        if (issuedOn.HasValue && expiresOn.HasValue && expiresOn.Value < issuedOn.Value)
            return Result.Failure<InstructorRegulatoryCredential>(InstructorRegulatoryCredentialErrors.InvalidValidityPeriod);

        return Result.Success(new InstructorRegulatoryCredential(
            InstructorRegulatoryCredentialId.New(), organizationId, instructorUserId, country, type, number,
            authority, jurisdiction, issuedOn, expiresOn, source, actorUserId, now));
    }

    public Result Verify(string verificationMethod, string? reason, UserId actorUserId, DateTimeOffset now)
    {
        if (!IsCurrent) return Result.Failure(InstructorRegulatoryCredentialErrors.NotCurrent);
        string method = (verificationMethod ?? string.Empty).Trim();
        if (method.Length is < 2 or > 80)
            return Result.Failure(InstructorRegulatoryCredentialErrors.VerificationMethodRequired);
        if (actorUserId.IsEmpty) return Result.Failure(InstructorRegulatoryCredentialErrors.InvalidOwner);

        Status = InstructorRegulatoryCredentialStatus.Verified;
        VerificationMethod = method;
        DecisionReason = NormalizeOptional(reason, 500);
        VerifiedAtUtc = now;
        VerifiedByUserId = actorUserId;
        return Result.Success();
    }

    public Result Reject(string reason, UserId actorUserId, DateTimeOffset now)
    {
        if (!IsCurrent) return Result.Failure(InstructorRegulatoryCredentialErrors.NotCurrent);
        string normalized = (reason ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > 500)
            return Result.Failure(InstructorRegulatoryCredentialErrors.DecisionReasonRequired);
        if (actorUserId.IsEmpty) return Result.Failure(InstructorRegulatoryCredentialErrors.InvalidOwner);

        Status = InstructorRegulatoryCredentialStatus.Rejected;
        DecisionReason = normalized;
        VerifiedAtUtc = now;
        VerifiedByUserId = actorUserId;
        return Result.Success();
    }

    public Result Supersede(InstructorRegulatoryCredentialId replacementId, DateTimeOffset now)
    {
        if (!IsCurrent) return Result.Failure(InstructorRegulatoryCredentialErrors.NotCurrent);
        if (replacementId.IsEmpty || replacementId == Id)
            return Result.Failure(InstructorRegulatoryCredentialErrors.InvalidReplacement);
        Status = InstructorRegulatoryCredentialStatus.Superseded;
        SupersededAtUtc = now;
        SupersededById = replacementId;
        return Result.Success();
    }

    public static string NormalizeToken(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    public static string NormalizeIdentifier(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static string? NormalizeOptionalToken(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value, int max) { var v = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); return v is not null && v.Length > max ? v[..max] : v; }
}

public readonly record struct InstructorRegulatoryCredentialId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static InstructorRegulatoryCredentialId New() => new(Guid.NewGuid());
}

public enum InstructorRegulatoryCredentialStatus { Declared = 0, Verified = 1, Rejected = 2, Superseded = 3 }
public enum InstructorRegulatoryCredentialSource { Manual = 0, Import = 1, ExternalProvider = 2 }
