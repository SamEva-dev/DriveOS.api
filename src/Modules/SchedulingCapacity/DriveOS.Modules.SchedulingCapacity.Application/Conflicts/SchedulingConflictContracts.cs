using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Conflicts;

public sealed record SchedulingConflictResponse(
    Guid Id,
    Guid BookingId,
    Guid? CalendarResourceId,
    Guid? ConflictingBookingId,
    int Type,
    int Priority,
    int Status,
    string CauseKey,
    string? Details,
    IReadOnlyCollection<int> SuggestedActions,
    DateTimeOffset DetectedAtUtc,
    int? Resolution,
    string? ResolutionReason,
    Guid? ResolvedByUserId,
    string? OverrideReason,
    string? OverrideRisk,
    Guid? OverrideApprovedByUserId,
    DateTimeOffset? OverrideExpiresAtUtc);

public sealed record SchedulingConflictScanResponse(
    Guid BookingId,
    int OpenConflicts,
    int CriticalConflicts,
    IReadOnlyCollection<SchedulingConflictResponse> Conflicts);

public interface ISchedulingConflictReadService
{
    Task<IReadOnlyCollection<SchedulingConflictResponse>> ListAsync(OrganizationId organizationId, int? status, int? priority, BookingId? bookingId, CancellationToken cancellationToken = default);
    Task<SchedulingConflictResponse?> GetAsync(OrganizationId organizationId, SchedulingConflictId conflictId, CancellationToken cancellationToken = default);
}

public interface ISchedulingConflictInboxService
{
    Task<SchedulingConflictScanResponse> RefreshAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default);
}
