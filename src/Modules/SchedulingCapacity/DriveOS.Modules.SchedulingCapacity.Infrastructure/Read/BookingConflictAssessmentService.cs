using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class BookingConflictAssessmentService(
    SchedulingCapacityDbContext dbContext,
    IOptions<SchedulingTransitionOptions> transitionOptions,
    IInstructorWorkforceAvailabilityGateway workforceAvailability)
    : IBookingConflictAssessmentService
{
    public async Task<BookingConflictAssessment> AssessAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        BookingTransitionPolicy policy = CreatePolicy(transitionOptions.Value);
        CalendarResourceId[] requestedResourceIds = booking.Resources.Select(x => x.CalendarResourceId).Distinct().ToArray();

        CalendarResource[] resources = await dbContext.CalendarResources
            .AsNoTracking()
            .Where(x => x.OrganizationId == booking.OrganizationId && requestedResourceIds.Contains(x.Id))
            .ToArrayAsync(cancellationToken);

        AvailabilityPlan[] plans = await dbContext.AvailabilityPlans
            .AsNoTracking()
            .Include(x => x.Rules)
            .Include(x => x.Exceptions)
            .Where(x => x.OrganizationId == booking.OrganizationId && requestedResourceIds.Contains(x.CalendarResourceId) && x.Status == AvailabilityPlanStatus.Active)
            .ToArrayAsync(cancellationToken);

        DateTimeOffset searchStart = booking.StartAtUtc.AddMinutes(-policy.MaximumTransitionMinutes);
        DateTimeOffset searchEnd = booking.EndAtUtc.AddMinutes(policy.MaximumTransitionMinutes);

        ExistingBookingResourceReservation[] reservations = await (
            from bookingResource in dbContext.BookingResources.AsNoTracking()
            join existingBooking in dbContext.Bookings.AsNoTracking() on bookingResource.BookingId equals existingBooking.Id
            where existingBooking.OrganizationId == booking.OrganizationId
                  && requestedResourceIds.Contains(bookingResource.CalendarResourceId)
                  && existingBooking.Id != booking.Id
                  && (existingBooking.Status == BookingStatus.Reserved ||
                      existingBooking.Status == BookingStatus.Confirmed ||
                      (existingBooking.Status == BookingStatus.Tentative && existingBooking.HoldExpiresAtUtc > DateTimeOffset.UtcNow))
                  && existingBooking.StartAtUtc < searchEnd
                  && searchStart < existingBooking.EndAtUtc
            select new ExistingBookingResourceReservation(
                existingBooking.Id,
                bookingResource.CalendarResourceId,
                existingBooking.StartAtUtc,
                existingBooking.EndAtUtc,
                bookingResource.Quantity,
                existingBooking.Status,
                existingBooking.BranchId))
            .ToArrayAsync(cancellationToken);

        var snapshots = new List<CalendarResourceSchedulingSnapshot>(resources.Length);
        foreach (CalendarResource resource in resources)
        {
            CalendarResourceSchedulingSnapshot snapshot = CreateSnapshot(resource, plans, booking.StartAtUtc, booking.EndAtUtc);
            if (resource.ResourceType == CalendarResourceType.Instructor && resource.ExternalResourceId != Guid.Empty)
            {
                InstructorWorkforceAvailabilityResult availability = await workforceAvailability.CheckAsync(
                    booking.OrganizationId, new UserId(resource.ExternalResourceId), booking.StartAtUtc, booking.EndAtUtc, booking.BranchId, resource.TimeZoneId, cancellationToken);
                if (availability.IsUnavailable)
                    snapshot = snapshot with { EffectiveCapacity = 0, UnavailabilityReason = availability.Reason };
            }
            snapshots.Add(snapshot);
        }

        return BookingConflictDetector.Assess(booking, snapshots, reservations, policy);
    }

    private static BookingTransitionPolicy CreatePolicy(SchedulingTransitionOptions options) => new BookingTransitionPolicy(
        options.InstructorSameBranchBufferMinutes,
        options.InstructorCrossBranchTravelMinutes,
        options.VehicleSameBranchBufferMinutes,
        options.VehicleCrossBranchTravelMinutes).Validate();

    private static CalendarResourceSchedulingSnapshot CreateSnapshot(CalendarResource resource, IReadOnlyCollection<AvailabilityPlan> plans, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc)
    {
        int effectiveCapacity = ResolveEffectiveCapacity(resource, plans, startAtUtc, endAtUtc);
        return new CalendarResourceSchedulingSnapshot(
            resource.Id,
            resource.Capacity,
            effectiveCapacity,
            resource.Status,
            resource.RestrictionReason,
            resource.UnavailabilityReason,
            resource.ResourceType,
            resource.BranchId);
    }

    private static int ResolveEffectiveCapacity(CalendarResource resource, IReadOnlyCollection<AvailabilityPlan> plans, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc)
    {
        if (resource.Status != CalendarResourceStatus.Active)
            return 0;

        TimeZoneInfo timeZone;
        try { timeZone = TimeZoneInfo.FindSystemTimeZoneById(resource.TimeZoneId); }
        catch (TimeZoneNotFoundException) { return 0; }
        catch (InvalidTimeZoneException) { return 0; }

        DateTimeOffset localStart = TimeZoneInfo.ConvertTime(startAtUtc, timeZone);
        DateTimeOffset localEnd = TimeZoneInfo.ConvertTime(endAtUtc, timeZone);
        DateOnly startDate = DateOnly.FromDateTime(localStart.DateTime);
        DateOnly endDate = DateOnly.FromDateTime(localEnd.DateTime);
        if (startDate != endDate)
            return 0;

        AvailabilityPlan? plan = plans
            .Where(x => x.CalendarResourceId == resource.Id && x.IsEffectiveOn(startDate))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault();
        if (plan is null)
            return 0;

        TimeOnly startTime = TimeOnly.FromDateTime(localStart.DateTime);
        TimeOnly endTime = TimeOnly.FromDateTime(localEnd.DateTime);
        return Math.Min(resource.Capacity, plan.ResolveCapacity(startDate, startTime, endTime));
    }
}
