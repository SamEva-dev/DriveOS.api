using DriveOS.Modules.ExamsCertification.Application.Registrations.Operations;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamOperationalPlanningGateway(ICalendarResourceReadService resources, IBookingReadService bookings) : IExamOperationalPlanningGateway
{
    public async Task<ExamOperationalPlanningAssessment> AssessAsync(OrganizationId organizationId, BranchId? departureBranchId, DateTimeOffset windowStartUtc, DateTimeOffset windowEndUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<BookingResponse> overlaps = await bookings.ListAsync(organizationId, departureBranchId, windowStartUtc, windowEndUtc, cancellationToken);
        var active = overlaps.Where(x => x.Status is (int)BookingStatus.Reserved or (int)BookingStatus.Confirmed or (int)BookingStatus.Tentative).ToArray();
        IReadOnlyCollection<CalendarResourceResponse> instructorResources = await resources.ListAsync(organizationId, CalendarResourceType.Instructor, departureBranchId, cancellationToken);
        IReadOnlyCollection<CalendarResourceResponse> vehicleResources = await resources.ListAsync(organizationId, CalendarResourceType.ExamVehicle, departureBranchId, cancellationToken);
        if (vehicleResources.Count == 0) vehicleResources = await resources.ListAsync(organizationId, CalendarResourceType.Vehicle, departureBranchId, cancellationToken);

        return new ExamOperationalPlanningAssessment(Map(instructorResources, active), Map(vehicleResources, active), Array.Empty<string>());
    }

    private static IReadOnlyList<ExamOperationalResourceCandidate> Map(IReadOnlyCollection<CalendarResourceResponse> resources, IReadOnlyCollection<BookingResponse> activeBookings)
    {
        return resources.Select(resource =>
        {
            string[] conflicts = activeBookings.Where(b => b.Resources.Any(r => r.CalendarResourceId == resource.Id))
                .Select(b => $"Booking:{b.Id}:{b.StartAtUtc:O}:{b.EndAtUtc:O}").ToArray();
            bool active = string.Equals(resource.Status, CalendarResourceStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase);
            return new ExamOperationalResourceCandidate(resource.Id, resource.ExternalResourceId, resource.DisplayName, resource.BranchId, active && conflicts.Length == 0, conflicts);
        }).OrderByDescending(x => x.IsAvailable).ThenBy(x => x.DisplayName).ToArray();
    }
}
