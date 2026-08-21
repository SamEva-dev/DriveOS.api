using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.TrainingDelivery.Application.GroupSessions;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class ConfirmedGroupBookingSourceGateway(IBookingReadService bookings, ICalendarResourceReadService resources) : IConfirmedGroupBookingSourceGateway
{
    public async Task<Result<ConfirmedGroupBookingSource>> GetAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken ct = default)
    {
        BookingResponse? booking = await bookings.GetAsync(organizationId, bookingId, ct);
        if (booking is null) return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.SourceBookingNotFound);
        if (booking.Status != (int)BookingStatus.Confirmed) return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.SourceBookingNotConfirmed);
        if (booking.BookingType is not ((int)BookingType.TheoryCourse) and not ((int)BookingType.TrainingSession) and not ((int)BookingType.Other))
            return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.SourceBookingWrongType);

        Guid[] students = booking.Participants.Where(x=>x.ParticipantType==(int)BookingParticipantType.Student).Select(x=>x.ExternalParticipantId).Where(x=>x!=Guid.Empty).Distinct().ToArray();
        if (students.Length < 2) return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.SourceBookingIncomplete);

        List<CalendarResourceResponse> resolved=[];
        foreach (BookingResourceResponse item in booking.Resources)
        {
            CalendarResourceResponse? r=await resources.GetAsync(organizationId,new CalendarResourceId(item.CalendarResourceId),ct);
            if(r is not null) resolved.Add(r);
        }
        CalendarResourceResponse[] instructors=resolved.Where(x=>string.Equals(x.ResourceType,nameof(CalendarResourceType.Instructor),StringComparison.OrdinalIgnoreCase)).ToArray();
        if(instructors.Length!=1||instructors[0].ExternalResourceId==Guid.Empty) return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.SourceBookingIncomplete);
        CalendarResourceResponse? room=resolved.FirstOrDefault(x=>string.Equals(x.ResourceType,nameof(CalendarResourceType.Room),StringComparison.OrdinalIgnoreCase));
        int capacity=room?.Capacity ?? students.Length;
        if(capacity<students.Length) return Result.Failure<ConfirmedGroupBookingSource>(GroupTrainingSessionErrors.CapacityExceeded);
        string program=string.IsNullOrWhiteSpace(booking.Title)?booking.TrainingCategory ?? "Collective training":booking.Title;
        return Result.Success(new ConfirmedGroupBookingSource(organizationId,bookingId,program,capacity,new UserId(instructors[0].ExternalResourceId),booking.BranchId.HasValue?new BranchId(booking.BranchId.Value):null,room?.Id,room?.DisplayName,booking.StartAtUtc,booking.EndAtUtc,booking.Objectives,students.Select(x=>new PersonId(x)).ToArray()));
    }
}
