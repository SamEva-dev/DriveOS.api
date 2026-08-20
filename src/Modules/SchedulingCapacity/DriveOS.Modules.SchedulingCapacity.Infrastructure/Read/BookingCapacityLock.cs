using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class BookingCapacityLock(SchedulingCapacityDbContext dbContext) : IBookingCapacityLock
{
    public async Task AcquireAsync(
        OrganizationId organizationId,
        IReadOnlyCollection<CalendarResourceId> resourceIds,
        CancellationToken cancellationToken = default)
    {
        if (!dbContext.HasActiveTransaction)
            throw new InvalidOperationException("A transaction is required before acquiring scheduling capacity locks.");

        foreach (CalendarResourceId resourceId in resourceIds.Distinct().OrderBy(x => x.Value))
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM scheduling_capacity.calendar_resources WHERE \"OrganizationId\" = {organizationId.Value} AND \"Id\" = {resourceId.Value} FOR UPDATE",
                cancellationToken);
        }
    }
}
