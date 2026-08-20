using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;

public sealed class RecurrenceOccurrence : Entity<RecurrenceOccurrenceId>
{
    private RecurrenceOccurrence() { }
    internal RecurrenceOccurrence(RecurrenceOccurrenceId id, RecurrenceSeriesId seriesId, DateOnly scheduledDate, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, int revision) : base(id)
    {
        SeriesId = seriesId;
        ScheduledDate = scheduledDate;
        StartAtUtc = startAtUtc.ToUniversalTime();
        EndAtUtc = endAtUtc.ToUniversalTime();
        Revision = revision;
        Status = RecurrenceOccurrenceStatus.Planned;
    }

    public RecurrenceSeriesId SeriesId { get; private set; }
    public DateOnly ScheduledDate { get; private set; }
    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }
    public int Revision { get; private set; }
    public RecurrenceOccurrenceStatus Status { get; private set; }
    public string? ExceptionReason { get; private set; }

    internal bool IsActive => Status is RecurrenceOccurrenceStatus.Planned or RecurrenceOccurrenceStatus.Rescheduled;
    internal void Cancel(string reason) { Status = RecurrenceOccurrenceStatus.Cancelled; ExceptionReason = reason; }
    internal void Supersede(string reason) { Status = RecurrenceOccurrenceStatus.Superseded; ExceptionReason = reason; }
    internal void Reschedule(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, string reason)
    {
        StartAtUtc = startAtUtc.ToUniversalTime();
        EndAtUtc = endAtUtc.ToUniversalTime();
        Status = RecurrenceOccurrenceStatus.Rescheduled;
        ExceptionReason = reason;
    }
}
