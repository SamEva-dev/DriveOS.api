using DriveOS.Modules.SchedulingCapacity.Application.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class SchedulingConflictReadService(SchedulingCapacityDbContext dbContext) : ISchedulingConflictReadService
{
    public async Task<IReadOnlyCollection<SchedulingConflictResponse>> ListAsync(
        OrganizationId organizationId,
        int? status,
        int? priority,
        BookingId? bookingId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SchedulingConflict> query = dbContext.SchedulingConflicts.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (status.HasValue && Enum.IsDefined(typeof(SchedulingConflictStatus), status.Value)) query = query.Where(x => x.Status == (SchedulingConflictStatus)status.Value);
        if (priority.HasValue && Enum.IsDefined(typeof(SchedulingConflictPriority), priority.Value)) query = query.Where(x => x.Priority == (SchedulingConflictPriority)priority.Value);
        if (bookingId.HasValue) query = query.Where(x => x.BookingId == bookingId.Value);
        SchedulingConflict[] conflicts = await query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.DetectedAtUtc).ToArrayAsync(cancellationToken);
        return conflicts.Select(Map).ToArray();
    }

    public async Task<SchedulingConflictResponse?> GetAsync(OrganizationId organizationId, SchedulingConflictId conflictId, CancellationToken cancellationToken = default)
    {
        SchedulingConflict? conflict = await dbContext.SchedulingConflicts.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == conflictId, cancellationToken);
        return conflict is null ? null : Map(conflict);
    }

    internal static SchedulingConflictResponse Map(SchedulingConflict conflict) => new(
        conflict.Id.Value,
        conflict.BookingId.Value,
        conflict.CalendarResourceId?.Value,
        conflict.ConflictingBookingId?.Value,
        (int)conflict.Type,
        (int)conflict.Priority,
        (int)conflict.Status,
        conflict.CauseKey,
        conflict.Details,
        ParseActions(conflict.SuggestedActions),
        conflict.DetectedAtUtc,
        conflict.Resolution.HasValue ? (int)conflict.Resolution.Value : null,
        conflict.ResolutionReason,
        conflict.ResolvedByUserId?.Value,
        conflict.OverrideReason,
        conflict.OverrideRisk,
        conflict.OverrideApprovedByUserId?.Value,
        conflict.OverrideExpiresAtUtc);

    private static IReadOnlyCollection<int> ParseActions(string value) => string.IsNullOrWhiteSpace(value)
        ? []
        : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.TryParse(x, out int result) ? result : 0).Where(x => x > 0).ToArray();
}
