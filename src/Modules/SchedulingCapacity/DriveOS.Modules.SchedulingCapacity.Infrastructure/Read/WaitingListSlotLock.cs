using DriveOS.Modules.SchedulingCapacity.Application.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class WaitingListSlotLock(SchedulingCapacityDbContext dbContext) : IWaitingListSlotLock
{
    public async Task AcquireAsync(OrganizationId organizationId, string slotKey, CancellationToken cancellationToken = default)
    {
        string lockKey = $"{organizationId.Value:N}:{slotKey}";
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))", cancellationToken);
    }
}
