using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Recurrences;

public sealed record CreateRecurrenceSeriesCommand(
    OrganizationId OrganizationId,
    BranchId? BranchId,
    int TargetType,
    int Frequency,
    int Interval,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? OccurrenceCount,
    IReadOnlyCollection<DayOfWeek> DaysOfWeek,
    TimeOnly LocalTime,
    int DurationMinutes,
    string TimeZoneId,
    string Title,
    int ResourceSelectionPolicy,
    IReadOnlyCollection<CreateRecurrenceResourceRequest> Resources) : ICommand<RecurrenceSeriesId>;

public sealed record GenerateRecurrenceOccurrencesCommand(OrganizationId OrganizationId, RecurrenceSeriesId SeriesId) : ICommand<int>;
public sealed record CancelRecurrenceOccurrenceCommand(OrganizationId OrganizationId, RecurrenceSeriesId SeriesId, RecurrenceOccurrenceId OccurrenceId, string Reason) : ICommand;
public sealed record RescheduleRecurrenceOccurrenceCommand(OrganizationId OrganizationId, RecurrenceSeriesId SeriesId, RecurrenceOccurrenceId OccurrenceId, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string Reason) : ICommand;
public sealed record ChangeFutureRecurrenceRuleCommand(OrganizationId OrganizationId, RecurrenceSeriesId SeriesId, DateOnly ApplyFrom, int Frequency, int Interval, DateOnly? EndDate, int? OccurrenceCount, IReadOnlyCollection<DayOfWeek> DaysOfWeek, TimeOnly LocalTime, int DurationMinutes) : ICommand;
public sealed record CancelRecurrenceSeriesCommand(OrganizationId OrganizationId, RecurrenceSeriesId SeriesId, string Reason) : ICommand;
