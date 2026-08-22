using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.SchedulingCapacity;

internal sealed class VehicleReplacementEligibilityGateway(IFleetVehicleComplianceReadService fleet) : IVehicleReplacementEligibilityGateway
{
    public async Task<VehicleReplacementEligibility> EvaluateAsync(OrganizationId organizationId, Guid vehicleId, BranchId? branchId,
        VehicleReplacementRequirements requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default)
    {
        FleetVehicleComplianceEvaluation r = await fleet.EvaluateAsync(organizationId, new VehicleId(vehicleId), branchId,
            new FleetVehicleComplianceRequirement(requirements.TrainingCategory, requirements.TransmissionType, requirements.DualControlRequired,
                requirements.RequiredAdaptations, requirements.EnergyType), startAtUtc, endAtUtc, cancellationToken);
        return new VehicleReplacementEligibility(r.IsEligible, r.TechnicalCompatibilityVerified, r.InsuranceVerified, r.MaintenanceVerified,
            r.BranchVerified, r.OwnershipVerified, r.BlockingReasons, r.Reviews);
    }
}
