using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository(SchedulingCapacityDbContext dbContext) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(BookingId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Resources)
            .Include(x => x.Participants)
            .Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations)
            .Include(x => x.AttendanceHistory)
            .Include(x => x.InstructorReplacementHistory)
            .SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);

    public Task<Booking?> GetByIdForUpdateAsync(BookingId id, OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.Bookings
            .Include(x => x.Resources)
            .Include(x => x.Participants)
            .Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations)
            .Include(x => x.AttendanceHistory)
            .Include(x => x.InstructorReplacementHistory)
            .SingleOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId, cancellationToken);


    public Task<Booking?> GetByCreationIdempotencyKeyAsync(OrganizationId organizationId, string idempotencyKey, CancellationToken cancellationToken = default) =>
        dbContext.Bookings
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.CreationIdempotencyKey == idempotencyKey, cancellationToken);

    public void Add(Booking booking) => dbContext.Bookings.Add(booking);
}
