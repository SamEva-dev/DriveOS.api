using DriveOS.Modules.SchedulingCapacity.Application.Availability;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class AvailabilityPlanReadService(SchedulingCapacityDbContext dbContext) : IAvailabilityPlanReadService
{
    public async Task<IReadOnlyCollection<AvailabilityPlanResponse>> ListForResourceAsync(OrganizationId organizationId, CalendarResourceId resourceId, CancellationToken cancellationToken = default)
    {
        var plans = await dbContext.AvailabilityPlans.AsNoTracking().Include(x => x.Rules).Include(x => x.Exceptions)
            .Where(x => x.OrganizationId == organizationId && x.CalendarResourceId == resourceId).OrderByDescending(x => x.EffectiveFrom).ToListAsync(cancellationToken);
        return plans.Select(Map).ToArray();
    }

    public async Task<AvailabilityPlanResponse?> GetAsync(OrganizationId organizationId, AvailabilityPlanId id, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.AvailabilityPlans.AsNoTracking().Include(x => x.Rules).Include(x => x.Exceptions)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);
        return plan is null ? null : Map(plan);
    }

    private static AvailabilityPlanResponse Map(DriveOS.Modules.SchedulingCapacity.Domain.Availability.AvailabilityPlan x) => new(
        x.Id.Value,
        x.CalendarResourceId.Value,
        x.EffectiveFrom,
        x.EffectiveTo,
        x.Status.ToString(),
        x.Rules.OrderBy(r => r.DayOfWeek).ThenBy(r => r.StartTime).Select(r => new AvailabilityRuleResponse(
            r.Id.Value,
            r.DayOfWeek.ToString(),
            r.StartTime,
            r.EndTime,
            r.Capacity,
            r.Type.ToString(),
            r.Source.ToString(),
            r.Priority,
            r.BranchId?.Value,
            r.TrainingCategory,
            r.ServiceArea)).ToArray(),
        x.Exceptions.OrderBy(e => e.Date).ThenBy(e => e.StartTime).Select(e => new AvailabilityExceptionResponse(
            e.Id.Value,
            e.Date,
            e.StartTime,
            e.EndTime,
            e.Type.ToString(),
            e.Source.ToString(),
            e.Priority,
            e.Capacity,
            e.Reason)).ToArray(),
        new AvailabilityPreferencesResponse(
            x.PreferredMeetingPoint,
            x.MaximumTravelDistanceKm,
            x.MinimumNoticeMinutes,
            x.TrainingFrequencyPerWeek,
            x.PreferredInstructorId?.Value,
            x.IntensiveRhythm,
            x.OneTimeGeolocationAllowed),
        x.CreatedAtUtc,
        x.LastModifiedAtUtc);
}
