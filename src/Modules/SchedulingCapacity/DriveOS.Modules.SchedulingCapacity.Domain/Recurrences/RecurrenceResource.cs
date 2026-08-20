using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;

public sealed class RecurrenceResource : Entity<RecurrenceResourceId>
{
    private RecurrenceResource() { }
    internal RecurrenceResource(RecurrenceResourceId id, RecurrenceSeriesId seriesId, CalendarResourceId calendarResourceId, int quantity) : base(id)
    {
        SeriesId = seriesId;
        CalendarResourceId = calendarResourceId;
        Quantity = quantity;
    }
    public RecurrenceSeriesId SeriesId { get; private set; }
    public CalendarResourceId CalendarResourceId { get; private set; }
    public int Quantity { get; private set; }
}
