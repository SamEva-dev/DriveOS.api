using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public static class BookingConflictDetector
{
    public static BookingConflictAssessment Assess(
        Booking booking,
        IReadOnlyCollection<CalendarResourceSchedulingSnapshot> resources,
        IReadOnlyCollection<ExistingBookingResourceReservation> existingReservations,
        BookingTransitionPolicy? transitionPolicy = null)
    {
        BookingTransitionPolicy policy = (transitionPolicy ?? BookingTransitionPolicy.Default).Validate();
        var conflicts = new List<BookingConflict>();

        foreach (BookingResource requested in booking.Resources)
        {
            CalendarResourceSchedulingSnapshot? resource = resources.SingleOrDefault(x => x.ResourceId == requested.CalendarResourceId);

            if (resource is null || resource.Status is CalendarResourceStatus.Unavailable or CalendarResourceStatus.Archived)
            {
                conflicts.Add(new BookingConflict(BookingConflictType.ResourceUnavailable, requested.CalendarResourceId, null, requested.Quantity, 0, resource?.UnavailabilityReason));
                continue;
            }

            if (resource.Status == CalendarResourceStatus.Restricted)
            {
                conflicts.Add(new BookingConflict(BookingConflictType.ResourceRestricted, requested.CalendarResourceId, null, requested.Quantity, resource.EffectiveCapacity, resource.RestrictionReason));
                continue;
            }

            if (resource.EffectiveCapacity <= 0)
            {
                conflicts.Add(new BookingConflict(BookingConflictType.OutsideAvailability, requested.CalendarResourceId, null, requested.Quantity, 0));
                continue;
            }

            ExistingBookingResourceReservation[] relevant = existingReservations
                .Where(x => x.ResourceId == requested.CalendarResourceId && x.BookingId != booking.Id && x.Status is BookingStatus.Reserved or BookingStatus.Confirmed or BookingStatus.Tentative)
                .ToArray();

            ExistingBookingResourceReservation[] overlaps = relevant
                .Where(x => Overlaps(booking.StartAtUtc, booking.EndAtUtc, x.StartAtUtc, x.EndAtUtc))
                .ToArray();

            int alreadyReserved = overlaps.Sum(x => x.Quantity);
            int effectiveCapacity = Math.Min(resource.ResourceCapacity, resource.EffectiveCapacity);
            int available = Math.Max(0, effectiveCapacity - alreadyReserved);

            if (requested.Quantity > available)
            {
                BookingConflictType type = effectiveCapacity == 1 && overlaps.Length > 0
                    ? BookingConflictType.OverlappingBooking
                    : BookingConflictType.CapacityExceeded;

                conflicts.Add(new BookingConflict(type, requested.CalendarResourceId, overlaps.Length == 1 ? overlaps[0].BookingId : null, requested.Quantity, available));
                continue;
            }

            if (resource.ResourceType is not (CalendarResourceType.Instructor or CalendarResourceType.Vehicle))
                continue;

            foreach (ExistingBookingResourceReservation adjacent in relevant.Where(x => !Overlaps(booking.StartAtUtc, booking.EndAtUtc, x.StartAtUtc, x.EndAtUtc)))
            {
                bool branchChanged = booking.BranchId != adjacent.BranchId;
                int requiredMinutes = policy.RequiredMinutes(resource.ResourceType, branchChanged);
                if (requiredMinutes <= 0)
                    continue;

                TimeSpan gap = adjacent.EndAtUtc <= booking.StartAtUtc
                    ? booking.StartAtUtc - adjacent.EndAtUtc
                    : adjacent.StartAtUtc - booking.EndAtUtc;

                if (gap < TimeSpan.Zero || gap.TotalMinutes >= requiredMinutes)
                    continue;

                conflicts.Add(new BookingConflict(
                    branchChanged ? BookingConflictType.TravelTimeViolation : BookingConflictType.TransitionBufferViolation,
                    requested.CalendarResourceId,
                    adjacent.BookingId,
                    requested.Quantity,
                    available,
                    $"Required transition: {requiredMinutes} min; actual gap: {Math.Max(0, (int)Math.Floor(gap.TotalMinutes))} min."));
            }
        }

        return new BookingConflictAssessment(booking.Id, booking.StartAtUtc, booking.EndAtUtc, conflicts);
    }

    public static bool Overlaps(DateTimeOffset firstStartUtc, DateTimeOffset firstEndUtc, DateTimeOffset secondStartUtc, DateTimeOffset secondEndUtc) =>
        firstStartUtc < secondEndUtc && secondStartUtc < firstEndUtc;
}
