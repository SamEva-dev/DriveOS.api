using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

// Anti-corruption boundary for Fleet & Resources. The current solution has no Fleet bounded context yet,
// therefore technical/compliance facts are never inferred from client input or CalendarResource display metadata.
internal sealed class VehicleReplacementEligibilityGateway : IVehicleReplacementEligibilityGateway
{
    public Task<VehicleReplacementEligibility> EvaluateAsync(OrganizationId organizationId, Guid vehicleId, BranchId? branchId,
        VehicleReplacementRequirements requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> reviews =
        [
            "fleet.vehicle.compatibility.external-review",
            "fleet.vehicle.insurance.external-review",
            "fleet.vehicle.maintenance.external-review",
            "fleet.vehicle.location.external-review",
            "fleet.vehicle.ownership.external-review"
        ];
        return Task.FromResult(new VehicleReplacementEligibility(false, false, false, false, false, false,
            ["fleet.vehicle.authoritative-data-unavailable"], reviews));
    }
}
