using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class SchedulingConflictInboxService(
    SchedulingCapacityDbContext dbContext,
    IBookingRepository bookingRepository,
    IBookingConflictAssessmentService assessmentService,
    ISchedulingConflictRepository conflictRepository,
    ISchedulingCapacityUnitOfWork unitOfWork) : ISchedulingConflictInboxService
{
    public async Task<SchedulingConflictScanResponse> RefreshAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default)
    {
        Booking? booking = await bookingRepository.GetByIdAsync(bookingId, organizationId, cancellationToken);
        if (booking is null) return new SchedulingConflictScanResponse(bookingId.Value, 0, 0, []);

        BookingConflictAssessment assessment = await assessmentService.AssessAsync(booking, cancellationToken);
        IReadOnlyCollection<SchedulingConflict> existing = await conflictRepository.GetOpenByBookingForUpdateAsync(organizationId, bookingId, cancellationToken);
        CalendarResourceId[] resourceIds = assessment.Conflicts.Select(x => x.CalendarResourceId).Distinct().ToArray();
        Dictionary<CalendarResourceId, CalendarResourceType> resourceTypes = await dbContext.CalendarResources.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && resourceIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.ResourceType, cancellationToken);

        var activeKeys = new HashSet<(CalendarResourceId?, BookingId?, SchedulingConflictType)>();
        var result = new List<SchedulingConflict>();

        foreach (BookingConflict item in assessment.Conflicts)
        {
            SchedulingConflictType type = MapType(item.Type, resourceTypes.GetValueOrDefault(item.CalendarResourceId));
            var key = ((CalendarResourceId?)item.CalendarResourceId, item.ConflictingBookingId, type);
            activeKeys.Add(key);
            SchedulingConflict? found = existing.FirstOrDefault(x => x.Matches(key.Item1, key.Item2, key.Item3));
            if (found is not null)
            {
                found.RefreshExpiredOverride(DateTimeOffset.UtcNow);
                result.Add(found);
                continue;
            }

            Result<SchedulingConflict> created = SchedulingConflict.Create(
                SchedulingConflictId.New(), organizationId, bookingId, item.CalendarResourceId, item.ConflictingBookingId,
                type, MapPriority(item.Type), CauseKey(type), BuildDetails(item), SuggestedActions(type));
            if (created.IsSuccess)
            {
                conflictRepository.Add(created.Value);
                result.Add(created.Value);
            }
        }

        foreach (SchedulingConflict stale in existing.Where(x => !activeKeys.Contains((x.CalendarResourceId, x.ConflictingBookingId, x.Type)))) stale.MarkObsolete();
        await unitOfWork.CommitAsync(cancellationToken);

        SchedulingConflict[] open = result.Where(x => x.Status is SchedulingConflictStatus.Open or SchedulingConflictStatus.ResolutionRequested or SchedulingConflictStatus.Overridden).ToArray();
        return new SchedulingConflictScanResponse(
            bookingId.Value,
            open.Length,
            open.Count(x => x.Priority == SchedulingConflictPriority.Critical),
            open.OrderByDescending(x => x.Priority).Select(SchedulingConflictReadService.Map).ToArray());
    }

    private static SchedulingConflictType MapType(BookingConflictType type, CalendarResourceType resourceType) => type switch
    {
        BookingConflictType.OverlappingBooking when resourceType == CalendarResourceType.Instructor => SchedulingConflictType.InstructorOverlap,
        BookingConflictType.OverlappingBooking when resourceType == CalendarResourceType.Student => SchedulingConflictType.StudentOverlap,
        BookingConflictType.OverlappingBooking when resourceType is CalendarResourceType.Vehicle or CalendarResourceType.ExamVehicle => SchedulingConflictType.VehicleOverlap,
        BookingConflictType.OverlappingBooking when resourceType == CalendarResourceType.Room => SchedulingConflictType.RoomOverlap,
        BookingConflictType.TravelTimeViolation => SchedulingConflictType.TravelTimeConflict,
        BookingConflictType.CapacityExceeded => SchedulingConflictType.CapacityConflict,
        BookingConflictType.ResourceUnavailable => SchedulingConflictType.ResourceUnavailable,
        BookingConflictType.ResourceRestricted => SchedulingConflictType.DocumentRestriction,
        BookingConflictType.OutsideAvailability => SchedulingConflictType.ResourceUnavailable,
        BookingConflictType.TransitionBufferViolation => SchedulingConflictType.WorkingTimeViolation,
        _ => SchedulingConflictType.Other
    };

    private static SchedulingConflictPriority MapPriority(BookingConflictType type) => type switch
    {
        BookingConflictType.ResourceUnavailable or BookingConflictType.ResourceRestricted => SchedulingConflictPriority.Critical,
        BookingConflictType.OverlappingBooking or BookingConflictType.CapacityExceeded or BookingConflictType.TravelTimeViolation => SchedulingConflictPriority.High,
        BookingConflictType.OutsideAvailability or BookingConflictType.TransitionBufferViolation => SchedulingConflictPriority.Normal,
        _ => SchedulingConflictPriority.Low
    };

    private static string CauseKey(SchedulingConflictType type) => $"scheduling.conflicts.{type}";
    private static string BuildDetails(BookingConflict item) => $"RequestedQuantity={item.RequestedQuantity};AvailableCapacity={item.AvailableCapacity};Reason={item.Reason ?? string.Empty}";

    private static IReadOnlyCollection<SchedulingConflictResolution> SuggestedActions(SchedulingConflictType type) => type switch
    {
        SchedulingConflictType.InstructorOverlap => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.ReassignInstructor, SchedulingConflictResolution.CancelBooking],
        SchedulingConflictType.VehicleOverlap => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.ReassignVehicle, SchedulingConflictResolution.CancelBooking],
        SchedulingConflictType.RoomOverlap or SchedulingConflictType.CapacityConflict => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.ChangeLocation, SchedulingConflictResolution.CancelBooking],
        SchedulingConflictType.TravelTimeConflict => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.ReassignInstructor, SchedulingConflictResolution.ChangeLocation],
        SchedulingConflictType.WorkingTimeViolation => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.AdjustMargin, SchedulingConflictResolution.RequestDecision],
        SchedulingConflictType.ResourceUnavailable or SchedulingConflictType.DocumentRestriction => [SchedulingConflictResolution.ReassignVehicle, SchedulingConflictResolution.ChangeLocation, SchedulingConflictResolution.Reschedule],
        _ => [SchedulingConflictResolution.Reschedule, SchedulingConflictResolution.RequestDecision]
    };
}
