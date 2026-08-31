using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

/// <summary>
/// Privacy-preserving cross-organization conflict check for external instructors.
/// It exposes only whether another active booking overlaps; no foreign tenant booking data is returned.
/// </summary>
public interface IExternalInstructorGlobalScheduleReadService
{
    Task<bool> HasConflictAsync(
        UserId instructorUserId,
        OrganizationId currentOrganizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        CancellationToken cancellationToken=default);
}
