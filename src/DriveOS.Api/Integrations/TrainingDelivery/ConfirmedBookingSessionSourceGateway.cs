using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class ConfirmedBookingSessionSourceGateway(IBookingReadService bookings, ICalendarResourceReadService resources) : IConfirmedBookingSessionSourceGateway
{
    public async Task<Result<ConfirmedBookingSessionSource>> GetAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default)
    {
        BookingResponse? booking = await bookings.GetAsync(organizationId, bookingId, cancellationToken);
        if (booking is null) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingNotFound);
        if (booking.Status != (int)BookingStatus.Confirmed) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingNotConfirmed);
        if (booking.BookingType != (int)BookingType.TrainingSession) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingWrongType);
        if (!booking.TrainingPathId.HasValue || booking.TrainingPathId.Value == Guid.Empty) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingIncomplete);

        Guid[] students = booking.Participants.Where(x => x.ParticipantType == (int)BookingParticipantType.Student).Select(x => x.ExternalParticipantId).Distinct().ToArray();
        if (students.Length != 1) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingIncomplete);

        var resolved = new List<CalendarResourceResponse>();
        foreach (BookingResourceResponse item in booking.Resources)
        {
            CalendarResourceResponse? resource = await resources.GetAsync(organizationId, new CalendarResourceId(item.CalendarResourceId), cancellationToken);
            if (resource is null) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingIncomplete);
            resolved.Add(resource);
        }

        Guid[] instructors = resolved.Where(x => string.Equals(x.ResourceType, nameof(CalendarResourceType.Instructor), StringComparison.OrdinalIgnoreCase)).Select(x => x.ExternalResourceId).Distinct().ToArray();
        if (instructors.Length != 1 || instructors[0] == Guid.Empty) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingIncomplete);

        Guid[] vehicles = resolved.Where(x => string.Equals(x.ResourceType, nameof(CalendarResourceType.Vehicle), StringComparison.OrdinalIgnoreCase) || string.Equals(x.ResourceType, nameof(CalendarResourceType.ExamVehicle), StringComparison.OrdinalIgnoreCase)).Select(x => x.ExternalResourceId).Distinct().ToArray();
        if (vehicles.Length > 1) return Result.Failure<ConfirmedBookingSessionSource>(TrainingSessionErrors.SourceBookingIncomplete);

        return Result.Success(new ConfirmedBookingSessionSource(
            organizationId, organizationId, organizationId, bookingId, new PersonId(students[0]), new TrainingPathId(booking.TrainingPathId.Value), new UserId(instructors[0]),
            booking.BranchId.HasValue ? new BranchId(booking.BranchId.Value) : null, vehicles.SingleOrDefault() == Guid.Empty ? null : vehicles.Single(),
            booking.StartAtUtc, booking.EndAtUtc, booking.TrainingCategory, booking.Objectives, booking.MeetingPoint, booking.PricingReference, booking.TrainingCreditAccountId.HasValue ? new TrainingCreditAccountId(booking.TrainingCreditAccountId.Value) : null, booking.CreditQuantity, booking.CreditReservationReference));
    }
}
