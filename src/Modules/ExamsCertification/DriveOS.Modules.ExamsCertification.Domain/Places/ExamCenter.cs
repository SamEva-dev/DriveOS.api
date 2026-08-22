using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

/// <summary>
/// Authoritative BC-11 representation of an examination centre. It is provider-neutral: external
/// identifiers are correlation metadata and never become the domain identifier of the centre.
/// </summary>
public sealed class ExamCenter : AggregateRoot<ExamCenterId>, IAuditableEntity
{
    private ExamCenter() { }

    private ExamCenter(ExamCenterId id, OrganizationId organizationId, string name, string countryCode,
        string timeZoneId, string? administrativeAreaCode, string? address, string? externalProviderCode,
        string? externalCenterId) : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        CountryCode = countryCode;
        TimeZoneId = timeZoneId;
        AdministrativeAreaCode = administrativeAreaCode;
        Address = address;
        ExternalProviderCode = externalProviderCode;
        ExternalCenterId = externalCenterId;
        Status = ExamCenterStatus.Active;
    }

    public OrganizationId OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string TimeZoneId { get; private set; } = "UTC";
    public string? AdministrativeAreaCode { get; private set; }
    public string? Address { get; private set; }
    public string? ExternalProviderCode { get; private set; }
    public string? ExternalCenterId { get; private set; }
    public ExamCenterStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamCenter> Create(ExamCenterId id, OrganizationId organizationId, string name,
        string countryCode, string timeZoneId, string? administrativeAreaCode, string? address,
        string? externalProviderCode, string? externalCenterId)
    {
        if (id.IsEmpty) return Result.Failure<ExamCenter>(ExamCenterErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamCenter>(ExamCenterErrors.InvalidOrganization);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200) return Result.Failure<ExamCenter>(ExamCenterErrors.InvalidName);
        if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2) return Result.Failure<ExamCenter>(ExamCenterErrors.InvalidCountry);
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Trim().Length > 100) return Result.Failure<ExamCenter>(ExamCenterErrors.InvalidTimeZone);

        return Result.Success(new ExamCenter(id, organizationId, name.Trim(), countryCode.Trim().ToUpperInvariant(),
            timeZoneId.Trim(), Normalize(administrativeAreaCode, 64), Normalize(address, 1000),
            Normalize(externalProviderCode, 100), Normalize(externalCenterId, 200)));
    }

    public Result ChangeStatus(ExamCenterStatus status, DateTimeOffset atUtc, UserId actor)
    {
        if (!Enum.IsDefined(status)) return Result.Failure(ExamCenterErrors.InvalidStatus);
        Status = status;
        SetModifiedAudit(atUtc, actor);
        return Result.Success();
    }

    /// <summary>
    /// Refreshes provider-owned descriptive metadata without changing the DriveOS identity of the centre.
    /// Returns true only when persisted metadata actually changed.
    /// </summary>
    public bool SynchronizeExternalProfile(string name, string countryCode, string timeZoneId,
        string? administrativeAreaCode, string? address, DateTimeOffset atUtc, UserId actor)
    {
        string normalizedName = string.IsNullOrWhiteSpace(name) ? Name : name.Trim()[..Math.Min(name.Trim().Length, 200)];
        string normalizedCountry = string.IsNullOrWhiteSpace(countryCode) ? CountryCode : countryCode.Trim().ToUpperInvariant();
        string normalizedTimeZone = string.IsNullOrWhiteSpace(timeZoneId) ? TimeZoneId : timeZoneId.Trim()[..Math.Min(timeZoneId.Trim().Length, 100)];
        string? normalizedArea = Normalize(administrativeAreaCode, 64);
        string? normalizedAddress = Normalize(address, 1000);

        bool changed = Name != normalizedName
            || CountryCode != normalizedCountry
            || TimeZoneId != normalizedTimeZone
            || AdministrativeAreaCode != normalizedArea
            || Address != normalizedAddress;

        if (!changed) return false;

        Name = normalizedName;
        CountryCode = normalizedCountry;
        TimeZoneId = normalizedTimeZone;
        AdministrativeAreaCode = normalizedArea;
        Address = normalizedAddress;
        SetModifiedAudit(atUtc, actor);
        return true;
    }

    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? byUserId) { if (CreatedAtUtc == default) { CreatedAtUtc = atUtc.ToUniversalTime(); CreatedByUserId = byUserId; } }
    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? byUserId) { LastModifiedAtUtc = atUtc.ToUniversalTime(); LastModifiedByUserId = byUserId; }
    private static string? Normalize(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}
