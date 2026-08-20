using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(BookingId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdForUpdateAsync(BookingId id, OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<Booking?> GetByCreationIdempotencyKeyAsync(OrganizationId organizationId, string idempotencyKey, CancellationToken cancellationToken = default);
    void Add(Booking booking);
}
