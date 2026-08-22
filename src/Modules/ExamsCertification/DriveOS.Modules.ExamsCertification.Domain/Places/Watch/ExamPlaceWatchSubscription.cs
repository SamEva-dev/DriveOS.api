using DriveOS.Modules.ExamsCertification.Domain.Places.Watch.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch;

/// <summary>
/// Defines a tenant-scoped recurring watch over an external exam-place provider. The subscription owns
/// its cadence and lifecycle only; provider credentials and transport remain infrastructure concerns.
/// </summary>
public sealed class ExamPlaceWatchSubscription : AggregateRoot<ExamPlaceWatchSubscriptionId>, IAuditableEntity
{
    private ExamPlaceWatchSubscription() { }

    private ExamPlaceWatchSubscription(
        ExamPlaceWatchSubscriptionId id,
        OrganizationId organizationId,
        string providerCode,
        string countryCode,
        string? administrativeAreaCode,
        string? examCategory,
        DateTimeOffset windowFromUtc,
        DateTimeOffset windowToUtc,
        int checkIntervalMinutes,
        string? centerExternalIds,
        DateTimeOffset nowUtc) : base(id)
    {
        OrganizationId = organizationId;
        ProviderCode = providerCode.Trim();
        CountryCode = countryCode.Trim().ToUpperInvariant();
        AdministrativeAreaCode = Normalize(administrativeAreaCode);
        ExamCategory = Normalize(examCategory);
        WindowFromUtc = windowFromUtc.ToUniversalTime();
        WindowToUtc = windowToUtc.ToUniversalTime();
        CheckIntervalMinutes = checkIntervalMinutes;
        CenterExternalIds = NormalizeCenters(centerExternalIds);
        Status = ExamPlaceWatchStatus.Active;
        NextCheckAtUtc = nowUtc.ToUniversalTime();
    }

    public OrganizationId OrganizationId { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public string? AdministrativeAreaCode { get; private set; }
    public string? ExamCategory { get; private set; }
    public DateTimeOffset WindowFromUtc { get; private set; }
    public DateTimeOffset WindowToUtc { get; private set; }
    public int CheckIntervalMinutes { get; private set; }
    /// <summary>Normalized semicolon-separated provider center identifiers; null means all centers in scope.</summary>
    public string? CenterExternalIds { get; private set; }
    public ExamPlaceWatchStatus Status { get; private set; }
    public DateTimeOffset NextCheckAtUtc { get; private set; }
    public DateTimeOffset? LastCheckedAtUtc { get; private set; }
    public DateTimeOffset? LastSuccessfulCheckAtUtc { get; private set; }
    public DateTimeOffset? LastAvailabilityDetectedAtUtc { get; private set; }
    public string? LastErrorCode { get; private set; }
    public int ConsecutiveFailureCount { get; private set; }
    public Guid? ProcessingLeaseToken { get; private set; }
    public DateTimeOffset? ProcessingLeaseUntilUtc { get; private set; }
    public uint Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public IReadOnlyCollection<string> GetCenterExternalIds() =>
        string.IsNullOrWhiteSpace(CenterExternalIds)
            ? Array.Empty<string>()
            : CenterExternalIds.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static Result<ExamPlaceWatchSubscription> Create(
        ExamPlaceWatchSubscriptionId id,
        OrganizationId organizationId,
        string providerCode,
        string countryCode,
        string? administrativeAreaCode,
        string? examCategory,
        DateTimeOffset windowFromUtc,
        DateTimeOffset windowToUtc,
        int checkIntervalMinutes,
        IReadOnlyCollection<string>? centerExternalIds,
        DateTimeOffset nowUtc)
    {
        if (id.IsEmpty) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidOrganization);
        if (string.IsNullOrWhiteSpace(providerCode)) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidProvider);
        if (string.IsNullOrWhiteSpace(countryCode)) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidCountry);
        if (windowToUtc <= windowFromUtc) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidPeriod);
        if (checkIntervalMinutes is < 1 or > 1440) return Result.Failure<ExamPlaceWatchSubscription>(ExamPlaceWatchErrors.InvalidInterval);

        string? centers = centerExternalIds is null ? null : string.Join(';', centerExternalIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x));
        var subscription = new ExamPlaceWatchSubscription(id, organizationId, providerCode, countryCode, administrativeAreaCode,
            examCategory, windowFromUtc, windowToUtc, checkIntervalMinutes, centers, nowUtc);
        subscription.RaiseDomainEvent(new ExamPlaceWatchSubscriptionCreatedDomainEvent(id, organizationId, subscription.ProviderCode));
        return Result.Success(subscription);
    }

    public Result Pause(UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamPlaceWatchStatus.Ended) return Result.Failure(ExamPlaceWatchErrors.Ended);
        if (Status != ExamPlaceWatchStatus.Active) return Result.Failure(ExamPlaceWatchErrors.NotActive);
        Status = ExamPlaceWatchStatus.Paused;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Resume(UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamPlaceWatchStatus.Ended) return Result.Failure(ExamPlaceWatchErrors.Ended);
        if (Status == ExamPlaceWatchStatus.Active) return Result.Failure(ExamPlaceWatchErrors.AlreadyActive);
        Status = ExamPlaceWatchStatus.Active;
        NextCheckAtUtc = nowUtc.ToUniversalTime();
        LastErrorCode = null;
        ConsecutiveFailureCount = 0;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public bool TryAcquireProcessingLease(Guid leaseToken, DateTimeOffset leaseUntilUtc, DateTimeOffset nowUtc)
    {
        if (Status != ExamPlaceWatchStatus.Active || NextCheckAtUtc > nowUtc) return false;
        if (ProcessingLeaseUntilUtc is { } currentLease && currentLease > nowUtc) return false;
        ProcessingLeaseToken = leaseToken;
        ProcessingLeaseUntilUtc = leaseUntilUtc.ToUniversalTime();
        return true;
    }

    public void RecordSuccessfulScan(DateTimeOffset checkedAtUtc, bool availabilityDetected, UserId actorUserId)
    {
        DateTimeOffset checkedAt = checkedAtUtc.ToUniversalTime();
        LastCheckedAtUtc = checkedAt;
        LastSuccessfulCheckAtUtc = checkedAt;
        if (availabilityDetected) LastAvailabilityDetectedAtUtc = checkedAt;
        LastErrorCode = null;
        ConsecutiveFailureCount = 0;
        NextCheckAtUtc = checkedAt.AddMinutes(CheckIntervalMinutes);
        ClearProcessingLease();
        SetModifiedAudit(checkedAt, actorUserId);
    }

    public void RecordFailedScan(DateTimeOffset checkedAtUtc, string errorCode, UserId actorUserId)
    {
        DateTimeOffset checkedAt = checkedAtUtc.ToUniversalTime();
        LastCheckedAtUtc = checkedAt;
        LastErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "integration.failed" : errorCode.Trim();
        ConsecutiveFailureCount++;
        int retryMinutes = Math.Min(CheckIntervalMinutes, Math.Max(1, 1 << Math.Min(ConsecutiveFailureCount - 1, 6)));
        NextCheckAtUtc = checkedAt.AddMinutes(retryMinutes);
        ClearProcessingLease();
        SetModifiedAudit(checkedAt, actorUserId);
    }

    public void RecordNewAvailability(ExamPlaceId examPlaceId, DateTimeOffset detectedAtUtc)
    {
        LastAvailabilityDetectedAtUtc = detectedAtUtc.ToUniversalTime();
        RaiseDomainEvent(new ExamPlaceAvailabilityDetectedDomainEvent(Id, OrganizationId, examPlaceId, detectedAtUtc.ToUniversalTime()));
    }

    private void ClearProcessingLease()
    {
        ProcessingLeaseToken = null;
        ProcessingLeaseUntilUtc = null;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeCenters(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
}
