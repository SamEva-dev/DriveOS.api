using DriveOS.Modules.SchedulingCapacity.Application.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class AvailabilityImpactAssessmentService(SchedulingCapacityDbContext dbContext)
    : IAvailabilityImpactAssessmentService
{
    public async Task<IReadOnlyCollection<ImpactedBookingResponse>> FindImpactedBookingsAsync(
        OrganizationId organizationId,
        CalendarResourceId resourceId,
        DateOnly localDate,
        TimeOnly localStart,
        TimeOnly localEnd,
        string timeZoneId,
        CancellationToken cancellationToken = default)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return [];
        }
        catch (InvalidTimeZoneException)
        {
            return [];
        }

        DateTime localStartDateTime = localDate.ToDateTime(localStart, DateTimeKind.Unspecified);
        DateTime localEndDateTime = localDate.ToDateTime(localEnd, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(localStartDateTime) || timeZone.IsInvalidTime(localEndDateTime))
            return [];

        DateTimeOffset startUtc = new(TimeZoneInfo.ConvertTimeToUtc(localStartDateTime, timeZone), TimeSpan.Zero);
        DateTimeOffset endUtc = new(TimeZoneInfo.ConvertTimeToUtc(localEndDateTime, timeZone), TimeSpan.Zero);

        var impacted = await (
            from bookingResource in dbContext.BookingResources.AsNoTracking()
            join booking in dbContext.Bookings.AsNoTracking() on bookingResource.BookingId equals booking.Id
            where booking.OrganizationId == organizationId
                  && bookingResource.CalendarResourceId == resourceId
                  && (booking.Status == BookingStatus.Reserved || booking.Status == BookingStatus.Confirmed)
                  && booking.StartAtUtc < endUtc
                  && startUtc < booking.EndAtUtc
            orderby booking.StartAtUtc
            select new
            {
                BookingId = booking.Id,
                booking.StartAtUtc,
                booking.EndAtUtc,
                booking.Status
            })
            .Distinct()
            .ToArrayAsync(cancellationToken);

        return impacted
            .Select(x => new ImpactedBookingResponse(
                x.BookingId.Value,
                x.StartAtUtc,
                x.EndAtUtc,
                x.Status.ToString()))
            .ToArray();
    }
}
