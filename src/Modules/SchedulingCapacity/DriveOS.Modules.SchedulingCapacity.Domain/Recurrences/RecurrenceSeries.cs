using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;

public sealed class RecurrenceSeries : AggregateRoot<RecurrenceSeriesId>, IAuditableEntity
{
    private readonly List<RecurrenceOccurrence> occurrences = [];
    private readonly List<RecurrenceResource> resources = [];
    private RecurrenceSeries() { }

    private RecurrenceSeries(RecurrenceSeriesId id, OrganizationId organizationId, BranchId? branchId, RecurrenceTargetType targetType,
        RecurrenceFrequency frequency, int interval, DateOnly startDate, DateOnly? endDate, int? occurrenceCount,
        TimeOnly localTime, int durationMinutes, string timeZoneId, string title, ResourceSelectionPolicy resourceSelectionPolicy,
        IReadOnlyCollection<DayOfWeek> daysOfWeek) : base(id)
    {
        OrganizationId = organizationId; BranchId = branchId; TargetType = targetType; Frequency = frequency; Interval = interval;
        StartDate = startDate; EndDate = endDate; OccurrenceCount = occurrenceCount; LocalTime = localTime; DurationMinutes = durationMinutes;
        TimeZoneId = timeZoneId; Title = title; ResourceSelectionPolicy = resourceSelectionPolicy; DaysOfWeek = string.Join(',', daysOfWeek.OrderBy(x => (int)x).Select(x => (int)x));
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId? BranchId { get; private set; }
    public RecurrenceTargetType TargetType { get; private set; }
    public RecurrenceFrequency Frequency { get; private set; }
    public int Interval { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public int? OccurrenceCount { get; private set; }
    public TimeOnly LocalTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public string Title { get; private set; } = string.Empty;
    public ResourceSelectionPolicy ResourceSelectionPolicy { get; private set; }
    public string DaysOfWeek { get; private set; } = string.Empty;
    public int Revision { get; private set; }
    public bool IsCancelled { get; private set; }
    public IReadOnlyCollection<RecurrenceOccurrence> Occurrences => occurrences;
    public IReadOnlyCollection<RecurrenceResource> Resources => resources;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<RecurrenceSeries> Create(RecurrenceSeriesId id, OrganizationId organizationId, BranchId? branchId,
        RecurrenceTargetType targetType, RecurrenceFrequency frequency, int interval, DateOnly startDate, DateOnly? endDate,
        int? occurrenceCount, IReadOnlyCollection<DayOfWeek> daysOfWeek, TimeOnly localTime, int durationMinutes,
        string timeZoneId, string title, ResourceSelectionPolicy resourceSelectionPolicy)
    {
        if (id.IsEmpty) return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidOrganization);
        if (!Enum.IsDefined(targetType) || !Enum.IsDefined(frequency) || !Enum.IsDefined(resourceSelectionPolicy) || interval is < 1 or > 52)
            return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidRule);
        if (endDate.HasValue && endDate.Value < startDate || occurrenceCount is <= 0 or > 1000 || (!endDate.HasValue && !occurrenceCount.HasValue))
            return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidPeriod);
        if (durationMinutes is < 5 or > 1440) return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidDuration);
        string normalizedTitle = title?.Trim() ?? string.Empty;
        if (normalizedTitle.Length is < 1 or > 200) return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidTitle);
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); } catch { return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidRule); }
        DayOfWeek[] normalizedDays = daysOfWeek.Distinct().ToArray();
        if (frequency == RecurrenceFrequency.Weekly && normalizedDays.Length == 0) return Result.Failure<RecurrenceSeries>(RecurrenceSeriesErrors.InvalidRule);

        var series = new RecurrenceSeries(id, organizationId, branchId, targetType, frequency, interval, startDate, endDate, occurrenceCount,
            localTime, durationMinutes, timeZoneId, normalizedTitle, resourceSelectionPolicy, normalizedDays);
        series.RaiseDomainEvent(new RecurrenceSeriesCreatedDomainEvent(id, organizationId));
        return Result.Success(series);
    }

    public Result AddResource(RecurrenceResourceId id, CalendarResourceId resourceId, int quantity)
    {
        if (id.IsEmpty || resourceId.IsEmpty || quantity is < 1 or > 10000) return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        if (resources.Any(x => x.CalendarResourceId == resourceId)) return Result.Success();
        resources.Add(new RecurrenceResource(id, Id, resourceId, quantity));
        return Result.Success();
    }

    public Result<int> GenerateOccurrences()
    {
        if (IsCancelled) return Result.Failure<int>(RecurrenceSeriesErrors.SeriesCancelled);
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        int before = occurrences.Count;
        foreach (DateOnly date in EnumerateDates())
        {
            if (occurrences.Any(x => x.ScheduledDate == date && x.Revision == Revision)) continue;
            DateTime localStart = date.ToDateTime(LocalTime, DateTimeKind.Unspecified);
            DateTimeOffset startUtc = new(TimeZoneInfo.ConvertTimeToUtc(localStart, zone), TimeSpan.Zero);
            DateTimeOffset endUtc = startUtc.AddMinutes(DurationMinutes);
            occurrences.Add(new RecurrenceOccurrence(RecurrenceOccurrenceId.New(), Id, date, startUtc, endUtc, Revision));
        }
        return Result.Success(occurrences.Count - before);
    }

    public Result CancelOccurrence(RecurrenceOccurrenceId occurrenceId, string reason)
    {
        RecurrenceOccurrence? item = occurrences.SingleOrDefault(x => x.Id == occurrenceId);
        if (item is null) return Result.Failure(RecurrenceSeriesErrors.OccurrenceNotFound);
        if (!item.IsActive) return Result.Failure(RecurrenceSeriesErrors.OccurrenceModificationNotAllowed);
        string normalized = NormalizeReason(reason); if (normalized.Length == 0) return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        item.Cancel(normalized); RaiseDomainEvent(new RecurrenceOccurrenceChangedDomainEvent(Id, occurrenceId, OrganizationId, item.Status)); return Result.Success();
    }

    public Result RescheduleOccurrence(RecurrenceOccurrenceId occurrenceId, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, string reason)
    {
        RecurrenceOccurrence? item = occurrences.SingleOrDefault(x => x.Id == occurrenceId);
        if (item is null) return Result.Failure(RecurrenceSeriesErrors.OccurrenceNotFound);
        if (!item.IsActive || endAtUtc <= startAtUtc) return Result.Failure(RecurrenceSeriesErrors.OccurrenceModificationNotAllowed);
        string normalized = NormalizeReason(reason); if (normalized.Length == 0) return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        item.Reschedule(startAtUtc, endAtUtc, normalized); RaiseDomainEvent(new RecurrenceOccurrenceChangedDomainEvent(Id, occurrenceId, OrganizationId, item.Status)); return Result.Success();
    }

    public Result ChangeFutureRule(DateOnly applyFrom, RecurrenceFrequency frequency, int interval, DateOnly? endDate, int? occurrenceCount,
        IReadOnlyCollection<DayOfWeek> daysOfWeek, TimeOnly localTime, int durationMinutes)
    {
        if (IsCancelled || applyFrom < StartDate || (endDate.HasValue && endDate.Value < applyFrom) || interval is < 1 or > 52 || durationMinutes is < 5 or > 1440)
            return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        DayOfWeek[] normalizedDays = daysOfWeek.Distinct().ToArray();
        if (frequency == RecurrenceFrequency.Weekly && normalizedDays.Length == 0) return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        foreach (RecurrenceOccurrence occurrence in occurrences.Where(x => x.ScheduledDate >= applyFrom && x.IsActive)) occurrence.Supersede("SeriesRuleChanged");
        Frequency = frequency; Interval = interval; StartDate = applyFrom; EndDate = endDate; OccurrenceCount = occurrenceCount; DaysOfWeek = string.Join(',', normalizedDays.OrderBy(x => (int)x).Select(x => (int)x)); LocalTime = localTime; DurationMinutes = durationMinutes; Revision++;
        RaiseDomainEvent(new RecurrenceSeriesChangedDomainEvent(Id, OrganizationId, Revision, applyFrom));
        return GenerateOccurrences().IsSuccess ? Result.Success() : Result.Failure(RecurrenceSeriesErrors.InvalidRule);
    }

    public Result CancelSeries(string reason)
    {
        string normalized = NormalizeReason(reason); if (normalized.Length == 0) return Result.Failure(RecurrenceSeriesErrors.InvalidRule);
        IsCancelled = true; foreach (RecurrenceOccurrence occurrence in occurrences.Where(x => x.IsActive)) occurrence.Cancel(normalized); return Result.Success();
    }

    private IEnumerable<DateOnly> EnumerateDates()
    {
        HashSet<DayOfWeek> weekdays = DaysOfWeek.Length == 0 ? [] : DaysOfWeek.Split(',').Select(int.Parse).Select(x => (DayOfWeek)x).ToHashSet();
        int produced = 0; DateOnly date = StartDate; DateOnly hardEnd = EndDate ?? StartDate.AddYears(5); int limit = OccurrenceCount ?? 1000;
        while (date <= hardEnd && produced < limit)
        {
            bool include = Frequency switch
            {
                RecurrenceFrequency.Daily => (date.DayNumber - StartDate.DayNumber) % Interval == 0,
                RecurrenceFrequency.Weekly => ((date.DayNumber - StartDate.DayNumber) / 7) % Interval == 0 && weekdays.Contains(date.DayOfWeek),
                RecurrenceFrequency.Monthly => date.Day == StartDate.Day && MonthsBetween(StartDate, date) % Interval == 0,
                _ => false
            };
            if (include) { yield return date; produced++; }
            date = date.AddDays(1);
        }
    }

    private static int MonthsBetween(DateOnly start, DateOnly current) => (current.Year - start.Year) * 12 + current.Month - start.Month;
    private static string NormalizeReason(string? reason) { string value = reason?.Trim() ?? string.Empty; return value.Length <= 500 ? value : string.Empty; }
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }
}
