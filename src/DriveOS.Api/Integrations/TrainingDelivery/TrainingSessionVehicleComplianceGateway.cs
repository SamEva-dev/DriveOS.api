using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionVehicleComplianceGateway(IFleetVehicleComplianceReadService fleet) : ITrainingSessionVehicleComplianceGateway
{
    public async Task<TrainingSessionVehicleCompliance> CheckAsync(OrganizationId organizationId, Guid vehicleId, BranchId? branchId,
        string? trainingCategory, DateTimeOffset plannedStartAtUtc, DateTimeOffset plannedEndAtUtc, CancellationToken cancellationToken = default)
    {
        FleetVehicleComplianceEvaluation r = await fleet.EvaluateAsync(organizationId, new VehicleId(vehicleId), branchId,
            new FleetVehicleComplianceRequirement(trainingCategory ?? string.Empty, null, true, [], null), plannedStartAtUtc, plannedEndAtUtc, cancellationToken);
        return new TrainingSessionVehicleCompliance(r.TechnicalCompatibilityVerified && r.InsuranceVerified && r.MaintenanceVerified && r.DocumentsVerified,
            r.IsEligible, r.BlockingReasons, r.Reviews);
    }
}
