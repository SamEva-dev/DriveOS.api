using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;

internal sealed class SchedulingConflictRepository(SchedulingCapacityDbContext dbContext) : ISchedulingConflictRepository
{
    public Task<SchedulingConflict?> GetByIdForUpdateAsync(SchedulingConflictId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.SchedulingConflicts.SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public async Task<IReadOnlyCollection<SchedulingConflict>> GetOpenByBookingForUpdateAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default) =>
        await dbContext.SchedulingConflicts.Where(x => x.OrganizationId == organizationId && x.BookingId == bookingId &&
            (x.Status == SchedulingConflictStatus.Open || x.Status == SchedulingConflictStatus.ResolutionRequested || x.Status == SchedulingConflictStatus.Overridden)).ToArrayAsync(cancellationToken);

    public void Add(SchedulingConflict conflict) => dbContext.SchedulingConflicts.Add(conflict);
}
