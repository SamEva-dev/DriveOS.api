using DriveOS.Modules.SchedulingCapacity.Application.Travel;
using DriveOS.Modules.SchedulingCapacity.Domain.Travel;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

// Deliberately returns no estimate until a concrete map/routing provider is configured.
// This prevents DriveOS from presenting straight-line or guessed durations as road travel times.
internal sealed class TravelRoutingGateway : ITravelRoutingGateway
{
    public Task<TravelRouteEstimate?> EstimateAsync(
        TravelLocation origin,
        TravelLocation destination,
        DateTimeOffset departureTimeUtc,
        TravelTransportMode transportMode,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<TravelRouteEstimate?>(null);
}
