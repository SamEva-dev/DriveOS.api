using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;

internal sealed class AvailabilityPlanRepository(SchedulingCapacityDbContext dbContext) : IAvailabilityPlanRepository
{
    public Task<AvailabilityPlan?> GetByIdAsync(AvailabilityPlanId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.AvailabilityPlans.AsNoTracking().Include(x => x.Rules).Include(x => x.Exceptions).SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public Task<AvailabilityPlan?> GetByIdForUpdateAsync(AvailabilityPlanId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.AvailabilityPlans.Include(x => x.Rules).Include(x => x.Exceptions).SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public async Task AddAsync(AvailabilityPlan availabilityPlan, CancellationToken cancellationToken = default) =>
        await dbContext.AvailabilityPlans.AddAsync(availabilityPlan, cancellationToken);
}
