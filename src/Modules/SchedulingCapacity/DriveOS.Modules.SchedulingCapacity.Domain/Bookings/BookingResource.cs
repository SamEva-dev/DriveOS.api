using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingResource
{
    private BookingResource() { }

    internal BookingResource(
        BookingResourceId id,
        BookingId bookingId,
        CalendarResourceId calendarResourceId,
        int quantity)
    {
        Id = id;
        BookingId = bookingId;
        CalendarResourceId = calendarResourceId;
        Quantity = quantity;
    }

    public BookingResourceId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public CalendarResourceId CalendarResourceId { get; private set; }
    public int Quantity { get; private set; }

    internal void ReplaceCalendarResource(CalendarResourceId calendarResourceId) => CalendarResourceId = calendarResourceId;
}
