using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;

internal sealed class CalendarResourceRepository(SchedulingCapacityDbContext dbContext) : ICalendarResourceRepository
{
    public Task<CalendarResource?> GetByIdAsync(CalendarResourceId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.CalendarResources.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public Task<CalendarResource?> GetByIdForUpdateAsync(CalendarResourceId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.CalendarResources.SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public Task<bool> ExistsByExternalReferenceAsync(OrganizationId organizationId, CalendarResourceType resourceType, Guid externalResourceId, CancellationToken cancellationToken = default) =>
        dbContext.CalendarResources.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.ResourceType == resourceType && x.ExternalResourceId == externalResourceId, cancellationToken);

    public void Add(CalendarResource resource) => dbContext.CalendarResources.Add(resource);
}
