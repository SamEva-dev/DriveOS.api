using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class BookingCreationIdempotencyLock(SchedulingCapacityDbContext dbContext) : IBookingCreationIdempotencyLock
{
    public async Task AcquireAsync(
        OrganizationId organizationId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("An active transaction is required before acquiring a booking idempotency lock.");

        string lockKey = $"{organizationId.Value:N}:{idempotencyKey.Trim()}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
            cancellationToken);
    }
}
