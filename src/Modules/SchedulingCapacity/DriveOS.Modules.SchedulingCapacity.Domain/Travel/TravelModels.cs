namespace DriveOS.Modules.SchedulingCapacity.Domain.Travel;

public enum TravelLocationMode
{
    ManualAddress = 1,
    ApproximateLocation = 2,
    OneTimePreciseLocation = 3,
    DuringMission = 4,
    ContinuousAuthorizedTracking = 5
}

public enum TravelTransportMode
{
    Driving = 1,
    Walking = 2,
    Cycling = 3,
    PublicTransport = 4,
    Other = 99
}

public enum TravelTimeSource
{
    Planned = 1,
    Actual = 2
}

public sealed record TravelLocation(
    TravelLocationMode Mode,
    string Label,
    string? Address,
    double? Latitude,
    double? Longitude,
    string? Purpose,
    DateTimeOffset? CapturedAtUtc,
    DateTimeOffset? ExpiresAtUtc)
{
    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
}

public sealed record TravelRouteEstimate(
    int EstimatedDurationMinutes,
    decimal? DistanceKilometers,
    string TrafficContext,
    string Source);

public sealed record TravelFeasibility(
    DateTimeOffset DepartureTimeUtc,
    DateTimeOffset ArrivalDeadlineUtc,
    TravelTimeSource DepartureTimeSource,
    TravelTimeSource ArrivalTimeSource,
    int AvailableMinutes,
    int EstimatedDurationMinutes,
    int RequiredBufferMinutes,
    int RequiredTotalMinutes,
    int MarginMinutes,
    bool IsFeasible,
    decimal? DistanceKilometers,
    string TrafficContext,
    string RouteSource);
