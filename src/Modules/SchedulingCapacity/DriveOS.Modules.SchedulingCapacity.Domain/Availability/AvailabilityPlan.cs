using DriveOS.Modules.SchedulingCapacity.Domain.Availability.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

/// <summary>
/// Aggregate root for availability Plan in the Scheduling & Capacity bounded context. It owns the consistency boundary and enforces the business invariants of this concept.
/// </summary>
public sealed class AvailabilityPlan : AggregateRoot<AvailabilityPlanId>, IAuditableEntity
{
    private readonly List<AvailabilityRule> rules = [];
    private readonly List<AvailabilityException> exceptions = [];

    private AvailabilityPlan() { }

    private AvailabilityPlan(
        AvailabilityPlanId id,
        OrganizationId organizationId,
        CalendarResourceId calendarResourceId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
        : base(id)
    {
        OrganizationId = organizationId;
        CalendarResourceId = calendarResourceId;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Status = AvailabilityPlanStatus.Draft;
    }

    /// <summary>
    /// Tenant organization that owns or scopes this domain object.
    /// </summary>
    public OrganizationId OrganizationId { get; private set; }
    /// <summary>
    /// Scheduling resource represented by this entity.
    /// </summary>
    public CalendarResourceId CalendarResourceId { get; private set; }
    /// <summary>
    /// Date or timestamp from which this definition becomes effective.
    /// </summary>
    public DateOnly EffectiveFrom { get; private set; }
    /// <summary>
    /// Date or timestamp until which this definition remains effective, when bounded.
    /// </summary>
    public DateOnly? EffectiveTo { get; private set; }
    /// <summary>
    /// Current lifecycle status of the object.
    /// </summary>
    public AvailabilityPlanStatus Status { get; private set; }
    /// <summary>
    /// Read-only collection of rules owned or referenced by this object.
    /// </summary>
    public IReadOnlyCollection<AvailabilityRule> Rules => rules;
    /// <summary>
    /// Read-only collection of exceptions owned or referenced by this object.
    /// </summary>
    public IReadOnlyCollection<AvailabilityException> Exceptions => exceptions;

    // Optional preference profile. Used mainly for student availability and kept on the
    // availability aggregate so Scheduling can rank slots without owning Student data.
    /// <summary>
    /// Preferred meeting point represented by this domain object.
    /// </summary>
    public string? PreferredMeetingPoint { get; private set; }
    /// <summary>
    /// Maximum travel distance km represented by this domain object.
    /// </summary>
    public decimal? MaximumTravelDistanceKm { get; private set; }
    /// <summary>
    /// Minimum notice minutes represented by this domain object.
    /// </summary>
    public int? MinimumNoticeMinutes { get; private set; }
    /// <summary>
    /// Training frequency per week represented by this domain object.
    /// </summary>
    public int? TrainingFrequencyPerWeek { get; private set; }
    /// <summary>
    /// Identifier of the preferred Instructor referenced by this object.
    /// </summary>
    public UserId? PreferredInstructorId { get; private set; }
    /// <summary>
    /// Intensive rhythm represented by this domain object.
    /// </summary>
    public bool IntensiveRhythm { get; private set; }
    /// <summary>
    /// One time geolocation allowed represented by this domain object.
    /// </summary>
    public bool OneTimeGeolocationAllowed { get; private set; }

    /// <summary>
    /// UTC timestamp at which the object was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }
    /// <summary>
    /// User who created the object, when known.
    /// </summary>
    public UserId? CreatedByUserId { get; private set; }
    /// <summary>
    /// UTC timestamp of the latest persisted modification.
    /// </summary>
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    /// <summary>
    /// User who performed the latest modification, when known.
    /// </summary>
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<AvailabilityPlan> Create(
        AvailabilityPlanId id,
        OrganizationId organizationId,
        CalendarResourceId calendarResourceId,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        if (id.IsEmpty)
            return Result.Failure<AvailabilityPlan>(AvailabilityPlanErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<AvailabilityPlan>(AvailabilityPlanErrors.InvalidOrganization);
        if (calendarResourceId.IsEmpty)
            return Result.Failure<AvailabilityPlan>(AvailabilityPlanErrors.InvalidResource);
        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
            return Result.Failure<AvailabilityPlan>(AvailabilityPlanErrors.InvalidEffectivePeriod);

        var plan = new AvailabilityPlan(id, organizationId, calendarResourceId, effectiveFrom, effectiveTo);
        plan.RaiseDomainEvent(new AvailabilityPlanCreatedDomainEvent(id, organizationId, calendarResourceId));
        return Result.Success(plan);
    }

    public Result<AvailabilityRuleId> AddRecurringRule(
        AvailabilityRuleId ruleId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        int capacity = 1,
        AvailabilityRuleType type = AvailabilityRuleType.Available,
        AvailabilityExceptionSource source = AvailabilityExceptionSource.SelfDeclared,
        int priority = 500,
        BranchId? branchId = null,
        string? trainingCategory = null,
        string? serviceArea = null)
    {
        if (Status != AvailabilityPlanStatus.Draft)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.ModificationNotAllowed);
        if (ruleId.IsEmpty)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidIdentifier);
        if (endTime <= startTime)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidTimeRange);
        if (!Enum.IsDefined(type))
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidRuleType);
        if (!Enum.IsDefined(source))
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidSource);
        if (priority is < 0 or > 1000)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidPriority);
        if (capacity is < 1 or > 10000)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidCapacity);

        string? normalizedCategory = Normalize(trainingCategory, 80);
        if (!string.IsNullOrWhiteSpace(trainingCategory) && normalizedCategory is null)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidTrainingCategory);
        string? normalizedServiceArea = Normalize(serviceArea, 200);
        if (!string.IsNullOrWhiteSpace(serviceArea) && normalizedServiceArea is null)
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.InvalidServiceArea);

        // Preferences may overlap actual availability. Two capacity-bearing rules may not overlap
        // for the same scope because ResolveCapacity must remain deterministic.
        if (type != AvailabilityRuleType.Preferred && rules.Any(x =>
                x.Type != AvailabilityRuleType.Preferred &&
                x.BranchId == branchId &&
                string.Equals(x.TrainingCategory, normalizedCategory, StringComparison.OrdinalIgnoreCase) &&
                x.Priority == priority &&
                x.Overlaps(dayOfWeek, startTime, endTime)))
            return Result.Failure<AvailabilityRuleId>(AvailabilityPlanErrors.RuleOverlap);

        rules.Add(new AvailabilityRule(
            ruleId,
            Id,
            dayOfWeek,
            startTime,
            endTime,
            capacity,
            type,
            source,
            priority,
            branchId,
            normalizedCategory,
            normalizedServiceArea));

        RaiseDomainEvent(new AvailabilityRuleAddedDomainEvent(Id, OrganizationId, ruleId, dayOfWeek));
        return Result.Success(ruleId);
    }

    public Result RemoveRecurringRule(AvailabilityRuleId ruleId)
    {
        if (Status != AvailabilityPlanStatus.Draft)
            return Result.Failure(AvailabilityPlanErrors.ModificationNotAllowed);

        AvailabilityRule? rule = rules.SingleOrDefault(x => x.Id == ruleId);
        if (rule is null)
            return Result.Failure(AvailabilityPlanErrors.RuleNotFound);

        rules.Remove(rule);
        return Result.Success();
    }

    public Result<AvailabilityExceptionId> AddException(
        AvailabilityExceptionId exceptionId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        AvailabilityExceptionType type,
        int? capacity,
        string? reason,
        AvailabilityExceptionSource? source = null,
        int? priority = null)
    {
        if (Status == AvailabilityPlanStatus.Archived)
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.ModificationNotAllowed);
        if (exceptionId.IsEmpty)
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidIdentifier);
        if (date < EffectiveFrom || (EffectiveTo.HasValue && date > EffectiveTo.Value))
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidEffectivePeriod);
        if (endTime <= startTime)
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidTimeRange);
        if (!Enum.IsDefined(type))
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidExceptionType);

        AvailabilityExceptionSource resolvedSource = source ?? AvailabilityExceptionPolicy.ResolveSource(type);
        if (!Enum.IsDefined(resolvedSource))
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidSource);

        int resolvedPriority = priority ?? AvailabilityExceptionPolicy.DefaultPriority(type, resolvedSource);
        if (resolvedPriority is < 0 or > 1000)
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidPriority);
        if (AvailabilityExceptionPolicy.IsAvailable(type) && (!capacity.HasValue || capacity.Value is < 1 or > 10000))
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidCapacity);
        if (AvailabilityExceptionPolicy.IsUnavailable(type) && capacity.HasValue)
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidCapacity);

        string? normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (normalizedReason is { Length: > 500 })
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.InvalidReason);
        if (exceptions.Any(x => x.Priority == resolvedPriority && x.Overlaps(date, startTime, endTime)))
            return Result.Failure<AvailabilityExceptionId>(AvailabilityPlanErrors.ExceptionOverlap);

        exceptions.Add(new AvailabilityException(
            exceptionId,
            Id,
            date,
            startTime,
            endTime,
            type,
            resolvedSource,
            resolvedPriority,
            capacity,
            normalizedReason));

        RaiseDomainEvent(new AvailabilityExceptionAddedDomainEvent(Id, OrganizationId, exceptionId, date, type));
        return Result.Success(exceptionId);
    }

    public Result RemoveException(AvailabilityExceptionId exceptionId)
    {
        if (Status == AvailabilityPlanStatus.Archived)
            return Result.Failure(AvailabilityPlanErrors.ModificationNotAllowed);

        AvailabilityException? exception = exceptions.SingleOrDefault(x => x.Id == exceptionId);
        if (exception is null)
            return Result.Failure(AvailabilityPlanErrors.ExceptionNotFound);

        exceptions.Remove(exception);
        return Result.Success();
    }

    public Result UpdatePreferences(
        string? preferredMeetingPoint,
        decimal? maximumTravelDistanceKm,
        int? minimumNoticeMinutes,
        int? trainingFrequencyPerWeek,
        UserId? preferredInstructorId,
        bool intensiveRhythm,
        bool oneTimeGeolocationAllowed)
    {
        if (Status == AvailabilityPlanStatus.Archived)
            return Result.Failure(AvailabilityPlanErrors.ModificationNotAllowed);
        if (maximumTravelDistanceKm is < 0 or > 1000)
            return Result.Failure(AvailabilityPlanErrors.InvalidTravelDistance);
        if (minimumNoticeMinutes is < 0 or > 43200)
            return Result.Failure(AvailabilityPlanErrors.InvalidMinimumNotice);
        if (trainingFrequencyPerWeek is < 1 or > 21)
            return Result.Failure(AvailabilityPlanErrors.InvalidTrainingFrequency);

        string? normalizedMeetingPoint = Normalize(preferredMeetingPoint, 500);
        if (!string.IsNullOrWhiteSpace(preferredMeetingPoint) && normalizedMeetingPoint is null)
            return Result.Failure(AvailabilityPlanErrors.InvalidMeetingPoint);

        PreferredMeetingPoint = normalizedMeetingPoint;
        MaximumTravelDistanceKm = maximumTravelDistanceKm;
        MinimumNoticeMinutes = minimumNoticeMinutes;
        TrainingFrequencyPerWeek = trainingFrequencyPerWeek;
        PreferredInstructorId = preferredInstructorId;
        IntensiveRhythm = intensiveRhythm;
        OneTimeGeolocationAllowed = oneTimeGeolocationAllowed;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status != AvailabilityPlanStatus.Draft || rules.All(x => x.Type == AvailabilityRuleType.Preferred))
            return Result.Failure(AvailabilityPlanErrors.ActivationNotAllowed);

        Status = AvailabilityPlanStatus.Active;
        RaiseDomainEvent(new AvailabilityPlanActivatedDomainEvent(Id, OrganizationId, CalendarResourceId));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == AvailabilityPlanStatus.Archived)
            return Result.Failure(AvailabilityPlanErrors.ArchiveNotAllowed);

        Status = AvailabilityPlanStatus.Archived;
        RaiseDomainEvent(new AvailabilityPlanArchivedDomainEvent(Id, OrganizationId, CalendarResourceId));
        return Result.Success();
    }

    public bool IsEffectiveOn(DateOnly date) =>
        Status == AvailabilityPlanStatus.Active &&
        date >= EffectiveFrom &&
        (!EffectiveTo.HasValue || date <= EffectiveTo.Value);

    public int ResolveCapacity(
        DateOnly localDate,
        TimeOnly localStart,
        TimeOnly localEnd,
        BranchId? branchId = null,
        string? trainingCategory = null)
    {
        if (!IsEffectiveOn(localDate) || localEnd <= localStart)
            return 0;

        AvailabilityException? exception = exceptions
            .Where(x => x.Covers(localDate, localStart, localEnd))
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault();

        if (exception is not null)
        {
            return AvailabilityExceptionPolicy.IsUnavailable(exception.Type)
                ? 0
                : exception.Capacity ?? 0;
        }

        AvailabilityRule? rule = rules
            .Where(x =>
                x.Type != AvailabilityRuleType.Preferred &&
                x.Covers(localDate.DayOfWeek, localStart, localEnd) &&
                (!x.BranchId.HasValue || x.BranchId == branchId) &&
                (string.IsNullOrWhiteSpace(x.TrainingCategory) || string.Equals(x.TrainingCategory, trainingCategory, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.Priority)
            .FirstOrDefault();

        return rule?.Capacity ?? 0;
    }

    public int ResolvePreferenceScore(
        DateOnly localDate,
        TimeOnly localStart,
        TimeOnly localEnd,
        BranchId? branchId = null,
        string? trainingCategory = null)
    {
        if (!IsEffectiveOn(localDate) || localEnd <= localStart)
            return 0;

        return rules
            .Where(x =>
                x.Type == AvailabilityRuleType.Preferred &&
                x.Covers(localDate.DayOfWeek, localStart, localEnd) &&
                (!x.BranchId.HasValue || x.BranchId == branchId) &&
                (string.IsNullOrWhiteSpace(x.TrainingCategory) || string.Equals(x.TrainingCategory, trainingCategory, StringComparison.OrdinalIgnoreCase)))
            .Select(x => x.Priority)
            .DefaultIfEmpty(0)
            .Max();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : null;
    }
}
