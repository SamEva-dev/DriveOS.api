using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public interface ISchedulingConflictRepository
{
    Task<SchedulingConflict?> GetByIdForUpdateAsync(SchedulingConflictId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SchedulingConflict>> GetOpenByBookingForUpdateAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default);
    void Add(SchedulingConflict conflict);
}
