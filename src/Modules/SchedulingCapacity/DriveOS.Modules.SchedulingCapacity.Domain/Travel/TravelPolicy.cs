namespace DriveOS.Modules.SchedulingCapacity.Domain.Travel;

public static class TravelPolicy
{
    public const int MaximumRequiredBufferMinutes = 180;
    public const int MaximumPreciseLocationLifetimeMinutes = 240;

    public static TravelFeasibility Evaluate(
        DateTimeOffset previousPlannedEndUtc,
        DateTimeOffset? previousActualEndUtc,
        DateTimeOffset nextPlannedStartUtc,
        DateTimeOffset? nextActualStartUtc,
        TravelRouteEstimate route,
        int requiredBufferMinutes)
    {
        if (requiredBufferMinutes is < 0 or > MaximumRequiredBufferMinutes)
            throw new ArgumentOutOfRangeException(nameof(requiredBufferMinutes));
        if (route.EstimatedDurationMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(route));

        DateTimeOffset departure = (previousActualEndUtc ?? previousPlannedEndUtc).ToUniversalTime();
        DateTimeOffset arrivalDeadline = (nextActualStartUtc ?? nextPlannedStartUtc).ToUniversalTime();
        int available = Math.Max(0, (int)Math.Floor((arrivalDeadline - departure).TotalMinutes));
        int requiredTotal = checked(route.EstimatedDurationMinutes + requiredBufferMinutes);
        int margin = available - requiredTotal;

        return new TravelFeasibility(
            departure,
            arrivalDeadline,
            previousActualEndUtc.HasValue ? TravelTimeSource.Actual : TravelTimeSource.Planned,
            nextActualStartUtc.HasValue ? TravelTimeSource.Actual : TravelTimeSource.Planned,
            available,
            route.EstimatedDurationMinutes,
            requiredBufferMinutes,
            requiredTotal,
            margin,
            margin >= 0,
            route.DistanceKilometers,
            route.TrafficContext,
            route.Source);
    }

    public static void ValidateLocation(TravelLocation location, DateTimeOffset nowUtc, bool allowContinuousTracking)
    {
        if (!Enum.IsDefined(location.Mode))
            throw new ArgumentOutOfRangeException(nameof(location));
        if (string.IsNullOrWhiteSpace(location.Label) || location.Label.Trim().Length > 200)
            throw new ArgumentException("A location label is required.", nameof(location));

        if (location.Mode == TravelLocationMode.ManualAddress)
        {
            if (string.IsNullOrWhiteSpace(location.Address))
                throw new ArgumentException("A manual location requires an address.", nameof(location));
            return;
        }

        if (!location.HasCoordinates || location.Latitude is < -90 or > 90 || location.Longitude is < -180 or > 180)
            throw new ArgumentException("A geolocated position requires valid coordinates.", nameof(location));
        if (string.IsNullOrWhiteSpace(location.Purpose))
            throw new ArgumentException("A geolocated position requires an explicit purpose.", nameof(location));
        if (!location.CapturedAtUtc.HasValue || !location.ExpiresAtUtc.HasValue)
            throw new ArgumentException("A geolocated position must be time bounded.", nameof(location));

        DateTimeOffset captured = location.CapturedAtUtc.Value.ToUniversalTime();
        DateTimeOffset expires = location.ExpiresAtUtc.Value.ToUniversalTime();
        if (expires <= captured || expires <= nowUtc.ToUniversalTime())
            throw new ArgumentException("The location authorization is expired or invalid.", nameof(location));
        if ((expires - captured).TotalMinutes > MaximumPreciseLocationLifetimeMinutes)
            throw new ArgumentException("The location authorization lifetime is too long.", nameof(location));
        if (location.Mode == TravelLocationMode.ContinuousAuthorizedTracking && !allowContinuousTracking)
            throw new InvalidOperationException("Continuous location tracking is disabled by policy.");
    }
}
