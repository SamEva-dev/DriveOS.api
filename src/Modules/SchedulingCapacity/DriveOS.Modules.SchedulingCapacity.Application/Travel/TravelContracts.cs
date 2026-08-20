using DriveOS.Modules.SchedulingCapacity.Domain.Travel;

namespace DriveOS.Modules.SchedulingCapacity.Application.Travel;

public sealed record TravelLocationRequest(
    TravelLocationMode Mode,
    string Label,
    string? Address,
    double? Latitude,
    double? Longitude,
    string? Purpose,
    DateTimeOffset? CapturedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed record EvaluateTravelRequest(
    TravelLocationRequest Origin,
    TravelLocationRequest Destination,
    DateTimeOffset PreviousPlannedEndUtc,
    DateTimeOffset? PreviousActualEndUtc,
    DateTimeOffset NextPlannedStartUtc,
    DateTimeOffset? NextActualStartUtc,
    int? RequiredBufferMinutes,
    TravelTransportMode TransportMode,
    int? ManualEstimatedDurationMinutes,
    decimal? ManualDistanceKilometers,
    string? ManualTrafficContext);

public sealed record TravelEvaluationResponse(
    string OriginLabel,
    string DestinationLabel,
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
    string RouteSource,
    bool PreciseLocationPersisted,
    string PrivacyNotice);

public sealed class TravelPlanningException : Exception
{
    public TravelPlanningException(string code, string messageKey, IReadOnlyDictionary<string, object?>? parameters = null)
        : base(messageKey)
    {
        Code = code;
        MessageKey = messageKey;
        Parameters = parameters ?? new Dictionary<string, object?>();
    }

    public string Code { get; }
    public string MessageKey { get; }
    public IReadOnlyDictionary<string, object?> Parameters { get; }
}
