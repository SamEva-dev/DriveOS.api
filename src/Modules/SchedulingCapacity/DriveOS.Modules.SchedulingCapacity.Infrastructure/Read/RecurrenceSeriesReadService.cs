using DriveOS.Modules.SchedulingCapacity.Application.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class RecurrenceSeriesReadService(SchedulingCapacityDbContext db) : IRecurrenceSeriesReadService
{
    public async Task<RecurrenceSeriesResponse?> GetAsync(OrganizationId organizationId, RecurrenceSeriesId id, CancellationToken cancellationToken = default)
    {
        var series = await db.RecurrenceSeries
            .AsNoTracking()
            .Include(x => x.Occurrences)
            .Include(x => x.Resources)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

        return series is null ? null : Map(series);
    }

    public async Task<IReadOnlyCollection<RecurrenceSeriesResponse>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default)
    {
        var series = await db.RecurrenceSeries
            .AsNoTracking()
            .Include(x => x.Occurrences)
            .Include(x => x.Resources)
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return series.Select(Map).ToArray();
    }

    private static RecurrenceSeriesResponse Map(DriveOS.Modules.SchedulingCapacity.Domain.Recurrences.RecurrenceSeries series) =>
        new(
            series.Id.Value,
            series.OrganizationId.Value,
            series.BranchId?.Value,
            series.TargetType.ToString(),
            series.Frequency.ToString(),
            series.Interval,
            series.StartDate,
            series.EndDate,
            series.OccurrenceCount,
            ParseDaysOfWeek(series.DaysOfWeek),
            series.LocalTime,
            series.DurationMinutes,
            series.TimeZoneId,
            series.Title,
            series.ResourceSelectionPolicy.ToString(),
            series.Revision,
            series.IsCancelled,
            series.Resources
                .OrderBy(x => x.CalendarResourceId.Value)
                .Select(x => new RecurrenceResourceResponse(x.Id.Value, x.CalendarResourceId.Value, x.Quantity))
                .ToArray(),
            series.Occurrences
                .OrderBy(x => x.StartAtUtc)
                .Select(x => new RecurrenceOccurrenceResponse(x.Id.Value, x.ScheduledDate, x.StartAtUtc, x.EndAtUtc, x.Status.ToString(), x.ExceptionReason, x.Revision))
                .ToArray());

    private static IReadOnlyCollection<int> ParseDaysOfWeek(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<int>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse)
                .OrderBy(x => x)
                .ToArray();
}
