using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class ExternalInstructorGlobalScheduleReadService(
    SchedulingCapacityDbContext db):IExternalInstructorGlobalScheduleReadService
{
    public Task<bool> HasConflictAsync(
        UserId instructorUserId,
        OrganizationId currentOrganizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        CancellationToken cancellationToken=default)
    {
        if(instructorUserId.IsEmpty||endAtUtc<=startAtUtc)
            return Task.FromResult(false);

        return (
            from resource in db.CalendarResources.AsNoTracking()
            join bookingResource in db.BookingResources.AsNoTracking()
                on resource.Id equals bookingResource.CalendarResourceId
            join booking in db.Bookings.AsNoTracking()
                on bookingResource.BookingId equals booking.Id
            where resource.ResourceType==CalendarResourceType.Instructor
                  && resource.ExternalResourceId==instructorUserId.Value
                  && booking.OrganizationId!=currentOrganizationId
                  && (booking.Status==BookingStatus.Reserved||
                      booking.Status==BookingStatus.Confirmed||
                      (booking.Status==BookingStatus.Tentative&&booking.HoldExpiresAtUtc>DateTimeOffset.UtcNow))
                  && booking.StartAtUtc<endAtUtc
                  && startAtUtc<booking.EndAtUtc
            select booking.Id)
            .AnyAsync(cancellationToken);
    }
}
