using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public sealed class SchedulingConflict : AggregateRoot<SchedulingConflictId>, IAuditableEntity
{
    private SchedulingConflict() { }

    private SchedulingConflict(
        SchedulingConflictId id,
        OrganizationId organizationId,
        BookingId bookingId,
        CalendarResourceId? resourceId,
        BookingId? conflictingBookingId,
        SchedulingConflictType type,
        SchedulingConflictPriority priority,
        string causeKey,
        string? details,
        string suggestedActions) : base(id)
    {
        OrganizationId = organizationId;
        BookingId = bookingId;
        CalendarResourceId = resourceId;
        ConflictingBookingId = conflictingBookingId;
        Type = type;
        Priority = priority;
        CauseKey = causeKey;
        Details = details;
        SuggestedActions = suggestedActions;
        Status = SchedulingConflictStatus.Open;
        DetectedAtUtc = DateTimeOffset.UtcNow;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BookingId BookingId { get; private set; }
    public CalendarResourceId? CalendarResourceId { get; private set; }
    public BookingId? ConflictingBookingId { get; private set; }
    public SchedulingConflictType Type { get; private set; }
    public SchedulingConflictPriority Priority { get; private set; }
    public SchedulingConflictStatus Status { get; private set; }
    public string CauseKey { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public string SuggestedActions { get; private set; } = string.Empty;
    public DateTimeOffset DetectedAtUtc { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public SchedulingConflictResolution? Resolution { get; private set; }
    public string? ResolutionReason { get; private set; }
    public UserId? ResolvedByUserId { get; private set; }
    public string? OverrideReason { get; private set; }
    public string? OverrideRisk { get; private set; }
    public UserId? OverrideApprovedByUserId { get; private set; }
    public DateTimeOffset? OverrideExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsOpen => Status is SchedulingConflictStatus.Open or SchedulingConflictStatus.ResolutionRequested;

    public static Result<SchedulingConflict> Create(
        SchedulingConflictId id,
        OrganizationId organizationId,
        BookingId bookingId,
        CalendarResourceId? resourceId,
        BookingId? conflictingBookingId,
        SchedulingConflictType type,
        SchedulingConflictPriority priority,
        string causeKey,
        string? details,
        IReadOnlyCollection<SchedulingConflictResolution> suggestedActions)
    {
        string normalizedCause = causeKey?.Trim() ?? string.Empty;
        if (id.IsEmpty) return Result.Failure<SchedulingConflict>(SchedulingConflictErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || bookingId.IsEmpty) return Result.Failure<SchedulingConflict>(SchedulingConflictErrors.InvalidOrganization);
        if (!Enum.IsDefined(type) || !Enum.IsDefined(priority) || normalizedCause.Length is < 1 or > 200)
            return Result.Failure<SchedulingConflict>(SchedulingConflictErrors.InvalidReason);

        string actions = string.Join(',', suggestedActions.Distinct().Where(x => Enum.IsDefined(typeof(SchedulingConflictResolution), x)).Select(x => (int)x));
        var conflict = new SchedulingConflict(id, organizationId, bookingId, resourceId, conflictingBookingId, type, priority, normalizedCause,
            Normalize(details, 1000), actions);
        conflict.RaiseDomainEvent(new SchedulingConflictDetectedDomainEvent(id, organizationId, bookingId, type, priority));
        return Result.Success(conflict);
    }

    public Result RequestResolution()
    {
        if (!IsOpen) return Result.Failure(SchedulingConflictErrors.AlreadyClosed);
        Status = SchedulingConflictStatus.ResolutionRequested;
        return Result.Success();
    }

    public Result Resolve(SchedulingConflictResolution resolution, string reason, UserId? resolvedByUserId)
    {
        if (!IsOpen && Status != SchedulingConflictStatus.Overridden) return Result.Failure(SchedulingConflictErrors.AlreadyClosed);
        string normalized = Normalize(reason, 1000) ?? string.Empty;
        if (!Enum.IsDefined(resolution) || normalized.Length == 0) return Result.Failure(SchedulingConflictErrors.InvalidReason);
        Resolution = resolution;
        ResolutionReason = normalized;
        ResolvedByUserId = resolvedByUserId;
        ResolvedAtUtc = DateTimeOffset.UtcNow;
        Status = SchedulingConflictStatus.Resolved;
        RaiseDomainEvent(new SchedulingConflictResolvedDomainEvent(Id, OrganizationId, BookingId, resolution));
        return Result.Success();
    }

    public bool RefreshExpiredOverride(DateTimeOffset nowUtc)
    {
        if (Status != SchedulingConflictStatus.Overridden || !OverrideExpiresAtUtc.HasValue) return false;
        if (OverrideExpiresAtUtc.Value > nowUtc.ToUniversalTime()) return false;

        Status = SchedulingConflictStatus.Open;
        LastModifiedAtUtc = nowUtc.ToUniversalTime();
        return true;
    }

    public Result Override(string reason, string risk, UserId approvedByUserId, DateTimeOffset expiresAtUtc)
    {
        if (!IsOpen) return Result.Failure(SchedulingConflictErrors.AlreadyClosed);
        if (Priority == SchedulingConflictPriority.Critical) return Result.Failure(SchedulingConflictErrors.CriticalOverrideForbidden);
        string normalizedReason = Normalize(reason, 1000) ?? string.Empty;
        string normalizedRisk = Normalize(risk, 1000) ?? string.Empty;
        DateTimeOffset expiry = expiresAtUtc.ToUniversalTime();
        if (approvedByUserId.IsEmpty || normalizedReason.Length == 0 || normalizedRisk.Length == 0 || expiry <= DateTimeOffset.UtcNow)
            return Result.Failure(SchedulingConflictErrors.InvalidOverride);
        OverrideReason = normalizedReason;
        OverrideRisk = normalizedRisk;
        OverrideApprovedByUserId = approvedByUserId;
        OverrideExpiresAtUtc = expiry;
        Status = SchedulingConflictStatus.Overridden;
        RaiseDomainEvent(new SchedulingConflictOverriddenDomainEvent(Id, OrganizationId, BookingId, approvedByUserId, expiry));
        return Result.Success();
    }

    public void MarkObsolete()
    {
        if (Status == SchedulingConflictStatus.Resolved) return;
        Status = SchedulingConflictStatus.Obsolete;
        ResolvedAtUtc ??= DateTimeOffset.UtcNow;
    }

    public bool Matches(CalendarResourceId? resourceId, BookingId? conflictingBookingId, SchedulingConflictType type) =>
        CalendarResourceId == resourceId && ConflictingBookingId == conflictingBookingId && Type == type;

    private static string? Normalize(string? value, int maxLength)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }
}
