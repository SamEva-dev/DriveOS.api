using DriveOS.Modules.SchedulingCapacity.Application.Travel;
using DriveOS.Modules.SchedulingCapacity.Domain.Travel;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class TravelPlanningService(
    ITravelRoutingGateway routingGateway,
    IOptions<SchedulingTravelOptions> options) : ITravelPlanningService
{
    public async Task<TravelEvaluationResponse> EvaluateAsync(EvaluateTravelRequest request, CancellationToken cancellationToken = default)
    {
        SchedulingTravelOptions policy = options.Value;
        TravelLocation origin = Map(request.Origin);
        TravelLocation destination = Map(request.Destination);
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

        ValidateLocation(origin, nowUtc, policy.AllowContinuousTracking, "origin");
        ValidateLocation(destination, nowUtc, policy.AllowContinuousTracking, "destination");

        int buffer = request.RequiredBufferMinutes ?? policy.DefaultSafetyBufferMinutes;
        if (buffer is < 0 or > TravelPolicy.MaximumRequiredBufferMinutes)
            throw new TravelPlanningException(
                "Scheduling.Travel.InvalidBuffer",
                "errors.schedulingCapacity.travel.invalidBuffer",
                new Dictionary<string, object?> { ["maximum"] = TravelPolicy.MaximumRequiredBufferMinutes });

        DateTimeOffset departure = (request.PreviousActualEndUtc ?? request.PreviousPlannedEndUtc).ToUniversalTime();
        TravelRouteEstimate? route;

        if (request.ManualEstimatedDurationMinutes.HasValue)
        {
            if (request.ManualEstimatedDurationMinutes.Value < 0)
                throw new TravelPlanningException(
                    "Scheduling.Travel.InvalidDuration",
                    "errors.schedulingCapacity.travel.invalidDuration");
            if (request.ManualDistanceKilometers is < 0)
                throw new TravelPlanningException(
                    "Scheduling.Travel.InvalidDistance",
                    "errors.schedulingCapacity.travel.invalidDistance");

            route = new TravelRouteEstimate(
                request.ManualEstimatedDurationMinutes.Value,
                request.ManualDistanceKilometers,
                string.IsNullOrWhiteSpace(request.ManualTrafficContext) ? "manual" : request.ManualTrafficContext.Trim(),
                "manual");
        }
        else
        {
            route = await routingGateway.EstimateAsync(origin, destination, departure, request.TransportMode, cancellationToken);
        }

        if (route is null)
            throw new TravelPlanningException(
                "Scheduling.Travel.RoutingUnavailable",
                "errors.schedulingCapacity.travel.routingUnavailable");

        TravelFeasibility result;
        try
        {
            result = TravelPolicy.Evaluate(
                request.PreviousPlannedEndUtc,
                request.PreviousActualEndUtc,
                request.NextPlannedStartUtc,
                request.NextActualStartUtc,
                route,
                buffer);
        }
        catch (ArgumentException)
        {
            throw new TravelPlanningException(
                "Scheduling.Travel.InvalidRequest",
                "errors.schedulingCapacity.travel.invalidRequest");
        }

        return new TravelEvaluationResponse(
            origin.Label.Trim(),
            destination.Label.Trim(),
            result.DepartureTimeUtc,
            result.ArrivalDeadlineUtc,
            result.DepartureTimeSource,
            result.ArrivalTimeSource,
            result.AvailableMinutes,
            result.EstimatedDurationMinutes,
            result.RequiredBufferMinutes,
            result.RequiredTotalMinutes,
            result.MarginMinutes,
            result.IsFeasible,
            result.DistanceKilometers,
            result.TrafficContext,
            result.RouteSource,
            false,
            "travel.privacy.notPersisted");
    }

    private static void ValidateLocation(TravelLocation location, DateTimeOffset nowUtc, bool allowContinuousTracking, string side)
    {
        try
        {
            TravelPolicy.ValidateLocation(location, nowUtc, allowContinuousTracking);
        }
        catch (InvalidOperationException)
        {
            throw new TravelPlanningException(
                "Scheduling.Travel.ContinuousTrackingDisabled",
                "errors.schedulingCapacity.travel.continuousTrackingDisabled");
        }
        catch (ArgumentException)
        {
            throw new TravelPlanningException(
                "Scheduling.Travel.InvalidLocation",
                "errors.schedulingCapacity.travel.invalidLocation",
                new Dictionary<string, object?> { ["location"] = side });
        }
    }

    private static TravelLocation Map(TravelLocationRequest request) => new(
        request.Mode,
        request.Label,
        request.Address,
        request.Latitude,
        request.Longitude,
        request.Purpose,
        request.CapturedAtUtc,
        request.ExpiresAtUtc);
}
