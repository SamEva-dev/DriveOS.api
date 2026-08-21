using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class BookingExecutionReadinessService(
    IBookingRepository repository,
    IBookingConflictAssessmentService conflictAssessmentService)
    : IBookingExecutionReadinessService
{
    public async Task<BookingExecutionReadinessResponse> CheckAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default)
    {
        Booking? booking = await repository.GetByIdAsync(bookingId, organizationId, cancellationToken);
        if (booking is null)
            return new BookingExecutionReadinessResponse(false, false, false, []);

        if (booking.Status != BookingStatus.Confirmed)
            return new BookingExecutionReadinessResponse(true, false, false, []);

        BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
        BookingConflictResponse[] conflicts = assessment.Conflicts
            .Select(x => new BookingConflictResponse(
                (int)x.Type,
                x.CalendarResourceId.Value,
                x.ConflictingBookingId?.Value,
                x.RequestedQuantity,
                x.AvailableCapacity,
                x.Reason))
            .ToArray();

        return new BookingExecutionReadinessResponse(true, true, assessment.IsConflictFree, conflicts);
    }
}
