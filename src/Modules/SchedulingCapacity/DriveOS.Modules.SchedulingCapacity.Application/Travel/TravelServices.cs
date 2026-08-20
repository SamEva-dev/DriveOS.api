using DriveOS.Modules.SchedulingCapacity.Domain.Travel;

namespace DriveOS.Modules.SchedulingCapacity.Application.Travel;

public interface ITravelRoutingGateway
{
    Task<TravelRouteEstimate?> EstimateAsync(
        TravelLocation origin,
        TravelLocation destination,
        DateTimeOffset departureTimeUtc,
        TravelTransportMode transportMode,
        CancellationToken cancellationToken = default);
}

public interface ITravelPlanningService
{
    Task<TravelEvaluationResponse> EvaluateAsync(EvaluateTravelRequest request, CancellationToken cancellationToken = default);
}
