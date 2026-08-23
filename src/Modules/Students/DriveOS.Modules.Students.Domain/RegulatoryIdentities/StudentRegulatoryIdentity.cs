using DriveOS.Modules.Students.Domain.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.RegulatoryIdentities;

/// <summary>
/// Aggregate root for a country-specific student identifier used by a regulatory authority.
/// It deliberately keeps national identifiers such as the French NEPH outside Student.
/// A replacement creates a new aggregate and supersedes the previous one so history is preserved.
/// </summary>
public sealed class StudentRegulatoryIdentity : AggregateRoot<StudentRegulatoryIdentityId>
{
    private StudentRegulatoryIdentity() { }

    private StudentRegulatoryIdentity(
        StudentRegulatoryIdentityId id,
        OrganizationId organizationId,
        PersonId studentId,
        string countryCode,
        string identifierType,
        string value,
        StudentRegulatoryIdentitySource source,
        UserId actorUserId,
        DateTimeOffset now)
        : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        CountryCode = countryCode;
        IdentifierType = identifierType;
        Value = value;
        Source = source;
        Status = StudentRegulatoryIdentityStatus.Declared;
        DeclaredAtUtc = now;
        DeclaredByUserId = actorUserId;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public string IdentifierType { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public StudentRegulatoryIdentitySource Source { get; private set; }
    public StudentRegulatoryIdentityStatus Status { get; private set; }
    public DateTimeOffset DeclaredAtUtc { get; private set; }
    public UserId DeclaredByUserId { get; private set; }
    public DateTimeOffset? VerifiedAtUtc { get; private set; }
    public UserId? VerifiedByUserId { get; private set; }
    public string? VerificationMethod { get; private set; }
    public string? DecisionReason { get; private set; }
    public DateTimeOffset? SupersededAtUtc { get; private set; }
    public StudentRegulatoryIdentityId? SupersededById { get; private set; }

    public bool IsCurrent => Status is StudentRegulatoryIdentityStatus.Declared or StudentRegulatoryIdentityStatus.Verified;

    public static Result<StudentRegulatoryIdentity> Declare(
        OrganizationId organizationId,
        PersonId studentId,
        string countryCode,
        string identifierType,
        string value,
        StudentRegulatoryIdentitySource source,
        UserId actorUserId,
        DateTimeOffset now)
    {
        string country = NormalizeCountry(countryCode);
        string type = NormalizeToken(identifierType);
        string normalizedValue = NormalizeValue(value);

        if (organizationId.IsEmpty || studentId.IsEmpty || actorUserId.IsEmpty)
            return Result.Failure<StudentRegulatoryIdentity>(StudentRegulatoryIdentityErrors.InvalidOwner);
        if (country.Length != 2 || type.Length is < 2 or > 40 || normalizedValue.Length is < 1 or > 100)
            return Result.Failure<StudentRegulatoryIdentity>(StudentRegulatoryIdentityErrors.InvalidIdentifier);

        StudentRegulatoryIdentity identity = new(
            StudentRegulatoryIdentityId.New(),
            organizationId,
            studentId,
            country,
            type,
            normalizedValue,
            source,
            actorUserId,
            now);

        identity.RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentRegulatoryIdentityId>(
            identity.Id,
            identity.StudentId,
            identity.OrganizationId,
            $"RegulatoryIdentityDeclared:{identity.CountryCode}:{identity.IdentifierType}"));

        return Result.Success(identity);
    }

    public Result Verify(string method, string? reason, UserId actorUserId, DateTimeOffset now)
    {
        if (!IsCurrent)
            return Result.Failure(StudentRegulatoryIdentityErrors.NotCurrent);
        string normalizedMethod = method?.Trim() ?? string.Empty;
        if (normalizedMethod.Length is < 2 or > 80)
            return Result.Failure(StudentRegulatoryIdentityErrors.VerificationMethodRequired);

        Status = StudentRegulatoryIdentityStatus.Verified;
        VerificationMethod = normalizedMethod;
        DecisionReason = NormalizeOptional(reason, 500);
        VerifiedAtUtc = now;
        VerifiedByUserId = actorUserId;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentRegulatoryIdentityId>(
            Id, StudentId, OrganizationId, $"RegulatoryIdentityVerified:{CountryCode}:{IdentifierType}"));
        return Result.Success();
    }

    public Result Reject(string reason, UserId actorUserId, DateTimeOffset now)
    {
        if (!IsCurrent)
            return Result.Failure(StudentRegulatoryIdentityErrors.NotCurrent);
        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 3 or > 500)
            return Result.Failure(StudentRegulatoryIdentityErrors.DecisionReasonRequired);

        Status = StudentRegulatoryIdentityStatus.Rejected;
        DecisionReason = normalizedReason;
        VerifiedAtUtc = now;
        VerifiedByUserId = actorUserId;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentRegulatoryIdentityId>(
            Id, StudentId, OrganizationId, $"RegulatoryIdentityRejected:{CountryCode}:{IdentifierType}"));
        return Result.Success();
    }

    public Result Supersede(StudentRegulatoryIdentityId replacementId, UserId actorUserId, DateTimeOffset now)
    {
        if (!IsCurrent)
            return Result.Failure(StudentRegulatoryIdentityErrors.NotCurrent);
        if (replacementId.IsEmpty || replacementId == Id)
            return Result.Failure(StudentRegulatoryIdentityErrors.InvalidReplacement);

        Status = StudentRegulatoryIdentityStatus.Superseded;
        SupersededAtUtc = now;
        SupersededById = replacementId;
        DecisionReason = "Superseded by a newer regulatory identifier.";
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentRegulatoryIdentityId>(
            Id, StudentId, OrganizationId, $"RegulatoryIdentitySuperseded:{CountryCode}:{IdentifierType}"));
        return Result.Success();
    }

    public static string NormalizeValue(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    public static string NormalizeCountry(string countryCode) => (countryCode ?? string.Empty).Trim().ToUpperInvariant();
    public static string NormalizeToken(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        string? normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return normalized is not null && normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }
}

public readonly record struct StudentRegulatoryIdentityId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static StudentRegulatoryIdentityId New() => new(Guid.NewGuid());
}

public enum StudentRegulatoryIdentityStatus
{
    Declared = 0,
    Verified = 1,
    Rejected = 2,
    Superseded = 3
}

public enum StudentRegulatoryIdentitySource
{
    Manual = 0,
    Import = 1,
    ExternalProvider = 2
}
