using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class BookingReadService(SchedulingCapacityDbContext dbContext) : IBookingReadService
{
    public async Task<BookingResponse?> GetAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default)
    {
        Booking? booking = await dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Resources)
            .Include(x => x.Participants)
            .Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations)
            .Include(x => x.AttendanceHistory)
            .Include(x => x.InstructorReplacementHistory).Include(x => x.VehicleReplacementHistory)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == bookingId, cancellationToken);

        return booking is null ? null : Map(booking);
    }

    public async Task<IReadOnlyCollection<BookingResponse>> ListAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Booking> query = dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Resources)
            .Include(x => x.Participants)
            .Include(x => x.RescheduleHistory)
            .Include(x => x.Cancellations)
            .Include(x => x.AttendanceHistory)
            .Include(x => x.InstructorReplacementHistory).Include(x => x.VehicleReplacementHistory)
            .Where(x => x.OrganizationId == organizationId);

        if (branchId.HasValue)
            query = query.Where(x => x.BranchId == branchId);
        if (fromUtc.HasValue)
            query = query.Where(x => x.EndAtUtc > fromUtc.Value.ToUniversalTime());
        if (toUtc.HasValue)
            query = query.Where(x => x.StartAtUtc < toUtc.Value.ToUniversalTime());

        Booking[] bookings = await query
            .OrderBy(x => x.StartAtUtc)
            .ToArrayAsync(cancellationToken);

        return bookings.Select(Map).ToArray();
    }

    private static BookingResponse Map(Booking booking) => new(
        booking.Id.Value,
        booking.OrganizationId.Value,
        booking.BranchId?.Value,
        (int)booking.BookingType,
        booking.StartAtUtc,
        booking.EndAtUtc,
        booking.Title,
        booking.TrainingPathId,
        booking.TrainingCategory,
        booking.Objectives,
        booking.MeetingPoint,
        booking.PricingReference,
        booking.TrainingCreditAccountId,
        booking.CreditQuantity,
        (int)booking.CreditReservationStatus,
        booking.CreditReservationReference,
        booking.Notes,
        (int)booking.NotificationPolicy,
        booking.HoldExpiresAtUtc,
        (int)booking.Status,
        booking.CancellationReason,
        booking.Resources.Select(x => new BookingResourceResponse(
            x.Id.Value,
            x.CalendarResourceId.Value,
            x.Quantity)).ToArray(),
        booking.Participants.Select(x => new BookingParticipantResponse(
            x.Id.Value,
            (int)x.ParticipantType,
            x.ExternalParticipantId)).ToArray(),
        booking.RescheduleHistory
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new BookingRescheduleHistoryResponse(
                x.Id.Value,
                x.OperationId,
                x.PreviousStartAtUtc,
                x.PreviousEndAtUtc,
                x.NewStartAtUtc,
                x.NewEndAtUtc,
                x.PreviousBranchId?.Value,
                x.NewBranchId?.Value,
                (int)x.PreviousStatus,
                x.Reason,
                x.ResourcesChanged,
                x.PreviousResourceFingerprint,
                x.NewResourceFingerprint,
                x.OccurredAtUtc))
            .ToArray(),
        booking.Cancellation is null ? null : new BookingCancellationResponse(
            booking.Cancellation.Id.Value,
            booking.Cancellation.OperationId,
            (int)booking.Cancellation.Initiator,
            booking.Cancellation.InitiatorId,
            (int)booking.Cancellation.ReasonCode,
            booking.Cancellation.ReasonDetails,
            booking.Cancellation.CancelledAtUtc,
            booking.Cancellation.NoticeDurationMinutes,
            booking.Cancellation.PolicyCode,
            booking.Cancellation.PolicyVersion,
            booking.Cancellation.PolicyExplanationKey,
            (int)booking.Cancellation.CreditDecision,
            (int)booking.Cancellation.FeeDecision,
            (int)booking.Cancellation.NotificationDecision,
            booking.Cancellation.ReplacementRequired,
            booking.Cancellation.OverrideApplied,
            booking.Cancellation.OverrideReason),
        booking.CurrentAttendance is null ? null : MapAttendance(booking.CurrentAttendance),
        booking.AttendanceHistory
            .OrderByDescending(x => x.RecordedAtUtc)
            .ThenByDescending(x => x.Id.Value)
            .Select(MapAttendance)
            .ToArray(),
        booking.InstructorReplacementHistory
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new BookingInstructorReplacementResponse(
                x.Id.Value, x.OperationId, x.PreviousInstructorId.Value, x.ReplacementInstructorId.Value,
                x.PreviousResourceId.Value, x.ReplacementResourceId.Value, (int)x.Mode, x.Reason, x.OccurredAtUtc, x.AccessExpiresAtUtc))
            .ToArray(),
        booking.VehicleReplacementHistory
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new BookingVehicleReplacementResponse(
                x.Id.Value, x.OperationId, x.PreviousVehicleId, x.ReplacementVehicleId,
                x.PreviousResourceId.Value, x.ReplacementResourceId.Value, (int)x.Mode, x.Reason, x.OccurredAtUtc))
            .ToArray());

    private static BookingAttendanceResponse MapAttendance(BookingAttendance x) => new(
        x.Id.Value, x.OperationId, x.SupersedesAttendanceId?.Value, (int)x.Status, x.RecordedAtUtc, x.RecordedBy.Value,
        x.ArrivalTimeUtc, x.DepartureTimeUtc, x.DelayMinutes, x.Reason, x.EvidenceDocumentId,
        (int)x.ChargeDecision, (int)x.CreditDecision, (int)x.FollowUpAction, x.OverrideApplied, x.OverrideReason);
}
