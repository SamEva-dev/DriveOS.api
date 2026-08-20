namespace DriveOS.Modules.SchedulingCapacity.Application.Recurrences;

public sealed record CreateRecurrenceResourceRequest(Guid CalendarResourceId, int Quantity);
public sealed record RecurrenceResourceResponse(Guid Id, Guid CalendarResourceId, int Quantity);
public sealed record RecurrenceOccurrenceResponse(Guid Id, DateOnly ScheduledDate, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string Status, string? ExceptionReason, int Revision);
public sealed record RecurrenceSeriesResponse(Guid Id, Guid OrganizationId, Guid? BranchId, string TargetType, string Frequency, int Interval, DateOnly StartDate, DateOnly? EndDate, int? OccurrenceCount, IReadOnlyCollection<int> DaysOfWeek, TimeOnly LocalTime, int DurationMinutes, string TimeZoneId, string Title, string ResourceSelectionPolicy, int Revision, bool IsCancelled, IReadOnlyCollection<RecurrenceResourceResponse> Resources, IReadOnlyCollection<RecurrenceOccurrenceResponse> Occurrences);
public sealed record RecurrenceOccurrencePreviewResponse(Guid OccurrenceId, DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string Status, string? ExceptionReason, bool IsConflictFree, IReadOnlyCollection<string> ConflictCodes);
public sealed record RecurrencePreviewResponse(Guid SeriesId, int TotalOccurrences, int ConfirmableOccurrences, int ConflictingOccurrences, int ExceptionOccurrences, IReadOnlyCollection<RecurrenceOccurrencePreviewResponse> Occurrences);
