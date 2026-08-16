using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Students;

public sealed class Student : AggregateRoot<PersonId>, IAuditableEntity
{
    private readonly List<StudentIdentityAuditEntry> identityAuditEntries = [];
    private Student() { }

    private Student(PersonId id, OrganizationId organizationId, string firstName,
        string lastName, string? email, string? phone) : base(id)
    {
        OrganizationId = organizationId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Status = StudentStatus.Active;
        IdentityVerificationStatus = IdentityVerificationStatus.Unverified;
    }

    public OrganizationId OrganizationId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? PreferredName { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? BirthPlace { get; private set; }
    public string? Nationality { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? PostalCode { get; private set; }
    public string? City { get; private set; }
    public string? CountryCode { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? TimeZone { get; private set; }
    public bool AllowEmail { get; private set; } = true;
    public bool AllowSms { get; private set; } = true;
    public bool AllowPhone { get; private set; } = true;
    public IdentityVerificationStatus IdentityVerificationStatus { get; private set; }
    public DateTimeOffset? IdentityVerifiedAtUtc { get; private set; }
    public UserId? IdentityVerifiedByUserId { get; private set; }
    public IReadOnlyCollection<StudentIdentityAuditEntry> IdentityAuditEntries => identityAuditEntries;
    public StudentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Student> Create(PersonId id, OrganizationId organizationId,
        string firstName, string lastName, string? email, string? phone)
    {
        if (id.IsEmpty) return Result.Failure<Student>(StudentErrors.InvalidId);
        if (organizationId.IsEmpty) return Result.Failure<Student>(StudentErrors.InvalidOrganization);
        string normalizedFirstName = firstName?.Trim() ?? string.Empty;
        string normalizedLastName = lastName?.Trim() ?? string.Empty;
        if (normalizedFirstName.Length == 0) return Result.Failure<Student>(StudentErrors.FirstNameRequired);
        if (normalizedLastName.Length == 0) return Result.Failure<Student>(StudentErrors.LastNameRequired);
        if (normalizedFirstName.Length > 100 || normalizedLastName.Length > 100 ||
            email?.Trim().Length > 254 || phone?.Trim().Length > 40)
            return Result.Failure<Student>(StudentErrors.IdentityTooLong);
        var student = new Student(id, organizationId, normalizedFirstName,
            normalizedLastName, Normalize(email), Normalize(phone));
        student.RaiseDomainEvent(new StudentCreatedDomainEvent(student.Id, student.OrganizationId));
        return Result.Success(student);
    }

    public Result UpdateIdentity(StudentIdentityData data, string? justification,
        UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        bool protectedIdentity = IdentityVerificationStatus is
            IdentityVerificationStatus.DocumentVerified or IdentityVerificationStatus.ExternallyVerified;
        string reason = justification?.Trim() ?? string.Empty;
        if (protectedIdentity && reason.Length < 10)
            return Result.Failure(StudentErrors.VerifiedIdentityJustificationRequired);
        if (string.IsNullOrWhiteSpace(data.LegalFirstName) || string.IsNullOrWhiteSpace(data.LegalLastName))
            return Result.Failure(StudentErrors.LegalNameRequired);
        if (data.LegalFirstName.Trim().Length > 100 || data.LegalLastName.Trim().Length > 100 ||
            data.PreferredName?.Trim().Length > 100 || data.Email?.Trim().Length > 254 ||
            data.Phone?.Trim().Length > 40 || data.BirthPlace?.Trim().Length > 150 ||
            data.Nationality?.Trim().Length > 80 || data.AddressLine1?.Trim().Length > 200 ||
            data.AddressLine2?.Trim().Length > 200 || data.PostalCode?.Trim().Length > 20 ||
            data.City?.Trim().Length > 100 || data.CountryCode?.Trim().Length > 3 ||
            data.PreferredLanguage?.Trim().Length > 10 || data.TimeZone?.Trim().Length > 100)
            return Result.Failure(StudentErrors.IdentityTooLong);

        FirstName = data.LegalFirstName.Trim(); LastName = data.LegalLastName.Trim();
        PreferredName = Normalize(data.PreferredName); BirthDate = data.BirthDate;
        BirthPlace = Normalize(data.BirthPlace); Nationality = Normalize(data.Nationality);
        Email = Normalize(data.Email); Phone = Normalize(data.Phone);
        AddressLine1 = Normalize(data.AddressLine1); AddressLine2 = Normalize(data.AddressLine2);
        PostalCode = Normalize(data.PostalCode); City = Normalize(data.City);
        CountryCode = Normalize(data.CountryCode)?.ToUpperInvariant();
        PreferredLanguage = Normalize(data.PreferredLanguage)?.ToLowerInvariant();
        TimeZone = Normalize(data.TimeZone);
        AllowEmail = data.AllowEmail; AllowSms = data.AllowSms; AllowPhone = data.AllowPhone;
        IdentityVerificationStatus = IdentityVerificationStatus.Declared;
        IdentityVerifiedAtUtc = null; IdentityVerifiedByUserId = null;
        AddIdentityAudit("IdentityUpdated", reason.Length == 0 ? "Declared identity update" : reason,
            actorUserId, occurredAtUtc);
        RaiseDomainEvent(new StudentIdentityChangedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result VerifyIdentity(IdentityVerificationStatus status, string justification,
        UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (status is not (IdentityVerificationStatus.DocumentVerified or
            IdentityVerificationStatus.ExternallyVerified))
            return Result.Failure(StudentErrors.InvalidVerificationStatus);
        string reason = justification?.Trim() ?? string.Empty;
        if (reason.Length < 10) return Result.Failure(StudentErrors.VerificationJustificationRequired);
        IdentityVerificationStatus = status; IdentityVerifiedAtUtc = occurredAtUtc;
        IdentityVerifiedByUserId = actorUserId;
        AddIdentityAudit("IdentityVerified", reason, actorUserId, occurredAtUtc);
        RaiseDomainEvent(new StudentIdentityVerifiedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public Result UpdateSelfServiceContact(string? email, string? phone, string? addressLine1,
        string? addressLine2, string? postalCode, string? city, string? countryCode,
        string? preferredLanguage, string? timeZone, bool allowEmail, bool allowSms,
        bool allowPhone, UserId actorUserId,
        DateTimeOffset occurredAtUtc)
    {
        if (email?.Trim().Length > 254 || phone?.Trim().Length > 40 ||
            addressLine1?.Trim().Length > 200 || addressLine2?.Trim().Length > 200 ||
            postalCode?.Trim().Length > 20 || city?.Trim().Length > 100 ||
            countryCode?.Trim().Length > 3 || preferredLanguage?.Trim().Length > 10 ||
            timeZone?.Trim().Length > 100)
            return Result.Failure(StudentErrors.IdentityTooLong);
        Email = Normalize(email); Phone = Normalize(phone); AddressLine1 = Normalize(addressLine1);
        AddressLine2 = Normalize(addressLine2); PostalCode = Normalize(postalCode); City = Normalize(city);
        CountryCode = Normalize(countryCode)?.ToUpperInvariant();
        PreferredLanguage = Normalize(preferredLanguage)?.ToLowerInvariant(); TimeZone = Normalize(timeZone);
        AllowEmail = allowEmail; AllowSms = allowSms; AllowPhone = allowPhone;
        AddIdentityAudit("SelfServiceContactUpdated", "Student self-service allowed fields update",
            actorUserId, occurredAtUtc);
        RaiseDomainEvent(new StudentIdentityChangedDomainEvent(Id, OrganizationId));
        return Result.Success();
    }

    public void Archive(UserId actor, DateTimeOffset now)
    {
        Status = StudentStatus.Archived;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new StudentStatusChangedDomainEvent(Id, OrganizationId, Status.ToString()));
    }

    public void RestoreAsActive(UserId actor, DateTimeOffset now)
    {
        Status = StudentStatus.Active;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new StudentStatusChangedDomainEvent(Id, OrganizationId, Status.ToString()));
    }

    public void RestoreAsSuspended(UserId actor, DateTimeOffset now)
    {
        Status = StudentStatus.Suspended;
        SetModifiedAudit(now, actor);
        RaiseDomainEvent(new StudentStatusChangedDomainEvent(Id, OrganizationId, Status.ToString()));
    }

    private void AddIdentityAudit(string action, string justification, UserId actor,
        DateTimeOffset occurredAtUtc) => identityAuditEntries.Add(new StudentIdentityAuditEntry(
            Guid.NewGuid(), OrganizationId, Id, action, justification, actor, occurredAtUtc));

    public void SetCreatedAudit(DateTimeOffset at, UserId? by)
    { if (CreatedAtUtc == default) { CreatedAtUtc = at; CreatedByUserId = by; } }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by)
    { LastModifiedAtUtc = at; LastModifiedByUserId = by; }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
