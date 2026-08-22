using DriveOS.Modules.ExamsCertification.Domain.Places.Events;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Places;

/// <summary>
/// A single allocatable examination capacity unit. External providers may expose a capacity greater
/// than one; the import layer must materialize that capacity as independent ExamPlace aggregates.
/// This keeps locking and anti-double-allocation invariants local and concurrency-safe.
/// </summary>
public sealed class ExamPlace : AggregateRoot<ExamPlaceId>, IAuditableEntity
{
    private ExamPlace() { }

    private ExamPlace(ExamPlaceId id, OrganizationId organizationId, ExamCenterId examCenterId,
        string examType, string licenseCategory, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc,
        string timeZoneId, ExamPlaceSource source, string providerCode, string? externalPlaceId,
        DateTimeOffset observedAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        ExamCenterId = examCenterId;
        ExamType = examType;
        LicenseCategory = licenseCategory;
        StartsAtUtc = startsAtUtc.ToUniversalTime();
        EndsAtUtc = endsAtUtc.ToUniversalTime();
        TimeZoneId = timeZoneId;
        Source = source;
        ProviderCode = providerCode;
        ExternalPlaceId = externalPlaceId;
        LastObservedAtUtc = observedAtUtc.ToUniversalTime();
        Status = ExamPlaceStatus.Available;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamCenterId ExamCenterId { get; private set; }
    public string ExamType { get; private set; } = string.Empty;
    public string LicenseCategory { get; private set; } = string.Empty;
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public ExamPlaceSource Source { get; private set; }
    public string ProviderCode { get; private set; } = string.Empty;
    public string? ExternalPlaceId { get; private set; }
    public DateTimeOffset LastObservedAtUtc { get; private set; }
    public ExamPlaceStatus Status { get; private set; }
    public Guid? HoldToken { get; private set; }
    public DateTimeOffset? HoldExpiresAtUtc { get; private set; }
    public UserId? HeldByUserId { get; private set; }
    public PersonId? AssignedStudentId { get; private set; }
    public ExamRegistrationId? ExamRegistrationId { get; private set; }
    public uint Version { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ExamPlace> Create(ExamPlaceId id, OrganizationId organizationId, ExamCenterId examCenterId,
        string examType, string licenseCategory, DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc,
        string timeZoneId, ExamPlaceSource source, string providerCode, string? externalPlaceId,
        DateTimeOffset observedAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidOrganization);
        if (examCenterId.IsEmpty) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidCenter);
        if (endsAtUtc <= startsAtUtc) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidPeriod);
        if (string.IsNullOrWhiteSpace(examType) || string.IsNullOrWhiteSpace(licenseCategory)) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidCategory);
        if (string.IsNullOrWhiteSpace(providerCode)) return Result.Failure<ExamPlace>(ExamPlaceErrors.InvalidProvider);

        var place = new ExamPlace(id, organizationId, examCenterId, examType.Trim(), licenseCategory.Trim(), startsAtUtc,
            endsAtUtc, string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId.Trim(), source, providerCode.Trim(),
            string.IsNullOrWhiteSpace(externalPlaceId) ? null : externalPlaceId.Trim(), observedAtUtc);
        place.RaiseDomainEvent(new ExamPlaceCreatedDomainEvent(id, organizationId, examCenterId));
        return Result.Success(place);
    }

    public Result Hold(Guid holdToken, DateTimeOffset expiresAtUtc, UserId actorUserId, DateTimeOffset nowUtc)
    {
        ReleaseExpiredHold(nowUtc);
        if (Status != ExamPlaceStatus.Available) return Result.Failure(ExamPlaceErrors.NotAvailable);
        if (holdToken == Guid.Empty || expiresAtUtc <= nowUtc) return Result.Failure(ExamPlaceErrors.InvalidPeriod);
        HoldToken = holdToken;
        HoldExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        HeldByUserId = actorUserId;
        Status = ExamPlaceStatus.Held;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new ExamPlaceHeldDomainEvent(Id, OrganizationId, holdToken, HoldExpiresAtUtc.Value));
        return Result.Success();
    }

    public Result ReleaseHold(Guid holdToken, UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamPlaceStatus.Held && HoldExpiresAtUtc <= nowUtc)
        {
            ReleaseExpiredHold(nowUtc);
            return Result.Failure(ExamPlaceErrors.HoldExpired);
        }
        if (Status != ExamPlaceStatus.Held) return Result.Failure(ExamPlaceErrors.NotAvailable);
        if (HoldToken != holdToken) return Result.Failure(ExamPlaceErrors.HoldTokenMismatch);
        ClearHold();
        Status = ExamPlaceStatus.Available;
        SetModifiedAudit(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Assign(Guid holdToken, PersonId studentId, ExamRegistrationId registrationId, UserId actorUserId, DateTimeOffset nowUtc)
    {
        if (Status == ExamPlaceStatus.Held && HoldExpiresAtUtc <= nowUtc)
        {
            ReleaseExpiredHold(nowUtc);
            return Result.Failure(ExamPlaceErrors.HoldExpired);
        }
        if (studentId.IsEmpty) return Result.Failure(ExamPlaceErrors.InvalidStudent);
        if (registrationId.IsEmpty) return Result.Failure(ExamPlaceErrors.InvalidRegistration);
        if (Status == ExamPlaceStatus.Assigned || AssignedStudentId is not null) return Result.Failure(ExamPlaceErrors.AlreadyAssigned);
        if (Status != ExamPlaceStatus.Held) return Result.Failure(ExamPlaceErrors.NotAvailable);
        if (HoldExpiresAtUtc <= nowUtc) return Result.Failure(ExamPlaceErrors.HoldExpired);
        if (HoldToken != holdToken) return Result.Failure(ExamPlaceErrors.HoldTokenMismatch);

        AssignedStudentId = studentId;
        ExamRegistrationId = registrationId;
        ClearHold();
        Status = ExamPlaceStatus.Assigned;
        SetModifiedAudit(nowUtc, actorUserId);
        RaiseDomainEvent(new ExamPlaceAssignedDomainEvent(Id, OrganizationId, studentId, registrationId));
        return Result.Success();
    }

    /// <summary>
    /// Reconciles an authoritative availability snapshot. Assigned/confirmed/consumed places keep their
    /// operational identity and allocation; an availability feed must never silently undo an allocation.
    /// </summary>
    public bool SynchronizeExternalAvailability(ExamCenterId examCenterId, string examType, string licenseCategory,
        DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc, string timeZoneId, DateTimeOffset observedAtUtc, UserId actorUserId)
    {
        if (endsAtUtc <= startsAtUtc) return false;

        DateTimeOffset normalizedStart = startsAtUtc.ToUniversalTime();
        DateTimeOffset normalizedEnd = endsAtUtc.ToUniversalTime();
        string normalizedExamType = examType.Trim();
        string normalizedCategory = licenseCategory.Trim();
        string normalizedTimeZone = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId.Trim();

        bool descriptiveChanged = ExamCenterId != examCenterId
            || ExamType != normalizedExamType
            || LicenseCategory != normalizedCategory
            || StartsAtUtc != normalizedStart
            || EndsAtUtc != normalizedEnd
            || TimeZoneId != normalizedTimeZone;

        bool reactivated = Status == ExamPlaceStatus.Expired;
        bool canRefreshSchedule = Status is ExamPlaceStatus.Available or ExamPlaceStatus.Expired;

        if (canRefreshSchedule && descriptiveChanged)
        {
            ExamCenterId = examCenterId;
            ExamType = normalizedExamType;
            LicenseCategory = normalizedCategory;
            StartsAtUtc = normalizedStart;
            EndsAtUtc = normalizedEnd;
            TimeZoneId = normalizedTimeZone;
        }

        if (reactivated) Status = ExamPlaceStatus.Available;
        Observe(observedAtUtc);

        if (descriptiveChanged && canRefreshSchedule || reactivated)
        {
            SetModifiedAudit(observedAtUtc, actorUserId);
            RaiseDomainEvent(new ExamPlaceAvailabilityChangedDomainEvent(Id, OrganizationId, Status, observedAtUtc.ToUniversalTime()));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Marks a place absent from an authoritative provider snapshot. Allocated places are deliberately
    /// preserved because an availability endpoint commonly stops returning a slot after reservation.
    /// </summary>
    public bool MarkUnavailableFromProvider(DateTimeOffset observedAtUtc, UserId actorUserId)
    {
        if (Status is ExamPlaceStatus.Assigned or ExamPlaceStatus.Confirmed or ExamPlaceStatus.Consumed or ExamPlaceStatus.Cancelled)
            return false;

        if (Status == ExamPlaceStatus.Expired) return false;

        ClearHold();
        Status = ExamPlaceStatus.Expired;
        SetModifiedAudit(observedAtUtc, actorUserId);
        RaiseDomainEvent(new ExamPlaceAvailabilityChangedDomainEvent(Id, OrganizationId, Status, observedAtUtc.ToUniversalTime()));
        return true;
    }

    public void Observe(DateTimeOffset observedAtUtc)
    {
        if (observedAtUtc > LastObservedAtUtc) LastObservedAtUtc = observedAtUtc.ToUniversalTime();
    }

    private void ReleaseExpiredHold(DateTimeOffset nowUtc)
    {
        if (Status == ExamPlaceStatus.Held && HoldExpiresAtUtc <= nowUtc)
        {
            ClearHold();
            Status = ExamPlaceStatus.Available;
        }
    }

    private void ClearHold() { HoldToken = null; HoldExpiresAtUtc = null; HeldByUserId = null; }
    public void SetCreatedAudit(DateTimeOffset atUtc, UserId? byUserId) { if (CreatedAtUtc == default) { CreatedAtUtc = atUtc.ToUniversalTime(); CreatedByUserId = byUserId; } }
    public void SetModifiedAudit(DateTimeOffset atUtc, UserId? byUserId) { LastModifiedAtUtc = atUtc.ToUniversalTime(); LastModifiedByUserId = byUserId; }
}
