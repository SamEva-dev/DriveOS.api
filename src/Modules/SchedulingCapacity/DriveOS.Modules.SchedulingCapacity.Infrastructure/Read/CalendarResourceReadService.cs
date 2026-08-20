using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class CalendarResourceReadService(SchedulingCapacityDbContext dbContext) : ICalendarResourceReadService
{
    public async Task<IReadOnlyCollection<CalendarResourceResponse>> ListAsync(OrganizationId organizationId, CalendarResourceType? resourceType, BranchId? branchId, CancellationToken cancellationToken = default)
    {
        IQueryable<CalendarResource> query = dbContext.CalendarResources.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (resourceType.HasValue) query = query.Where(x => x.ResourceType == resourceType.Value);
        if (branchId.HasValue) query = query.Where(x => x.BranchId == branchId.Value);
        return await query.OrderBy(x => x.DisplayName).Select(x => Map(x)).ToListAsync(cancellationToken);
    }

    public async Task<CalendarResourceResponse?> GetAsync(OrganizationId organizationId, CalendarResourceId id, CancellationToken cancellationToken = default)
    {
        CalendarResource? entity = await dbContext.CalendarResources.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    private static CalendarResourceResponse Map(CalendarResource x) => new(
        x.Id.Value, x.BranchId.HasValue ? x.BranchId.Value.Value : null, x.ResourceType.ToString(), x.ExternalResourceId, x.DisplayName,
        x.Capacity, x.TimeZoneId, x.Status.ToString(), x.RestrictionReason, x.UnavailabilityReason, x.CreatedAtUtc, x.LastModifiedAtUtc);
}
