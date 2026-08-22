using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FleetResources.Infrastructure.Read;

internal sealed class FleetVehicleComplianceReadService(IVehicleRepository repository) : IFleetVehicleComplianceReadService
{
    public async Task<FleetVehicleComplianceEvaluation> EvaluateAsync(OrganizationId organizationId, VehicleId vehicleId, BranchId? branchId,
        FleetVehicleComplianceRequirement requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default)
    {
        Vehicle? vehicle = await repository.GetByIdAsync(organizationId, vehicleId, cancellationToken);
        if (vehicle is null) return new(false, false, false, false, false, false, false, false, false,
            ["fleet.vehicle.not-found"], []);

        var reasons = new List<string>();
        var reviews = new List<string>();
        bool category = vehicle.SupportsLicenseCategory(requirements.TrainingCategory);
        bool transmission = string.IsNullOrWhiteSpace(requirements.TransmissionType) || string.Equals(vehicle.TransmissionType, requirements.TransmissionType, StringComparison.OrdinalIgnoreCase);
        bool dual = !requirements.DualControlRequired || vehicle.DualControl;
        bool adaptations = vehicle.SupportsAdaptations(requirements.RequiredAdaptations);
        bool energy = string.IsNullOrWhiteSpace(requirements.EnergyType) || string.Equals(vehicle.EnergyType, requirements.EnergyType, StringComparison.OrdinalIgnoreCase);
        bool technical = category && transmission && dual && adaptations && energy && vehicle.TechnicalComplianceVerified;
        if (!category) reasons.Add("fleet.vehicle.category-incompatible");
        if (!transmission) reasons.Add("fleet.vehicle.transmission-incompatible");
        if (!dual) reasons.Add("fleet.vehicle.dual-control-required");
        if (!adaptations) reasons.Add("fleet.vehicle.adaptations-missing");
        if (!energy) reasons.Add("fleet.vehicle.energy-incompatible");
        if (!vehicle.TechnicalComplianceVerified) reasons.Add("fleet.vehicle.technical-compliance-unverified");

        bool insurance = vehicle.InsuranceValidUntilUtc is { } insuranceUntil && insuranceUntil >= endAtUtc.ToUniversalTime();
        if (!insurance) reasons.Add("fleet.vehicle.insurance-invalid");
        bool maintenance = !vehicle.MaintenanceBlocking && !(vehicle.NextMaintenanceDueAtUtc is { } due && due <= endAtUtc.ToUniversalTime());
        if (!maintenance) reasons.Add("fleet.vehicle.maintenance-blocking");
        bool documents = vehicle.DocumentsCompliant;
        if (!documents) reasons.Add("fleet.vehicle.documents-non-compliant");
        bool operational = vehicle.IsOperationalFor(startAtUtc, endAtUtc);
        if (!operational) reasons.Add("fleet.vehicle.not-operational");
        bool branch = branchId is null || vehicle.BranchId is null || vehicle.BranchId == branchId;
        if (!branch) reasons.Add("fleet.vehicle.branch-incompatible");
        bool ownership = vehicle.OwnerOrganizationId == organizationId || vehicle.ProviderOrganizationId is not null;
        if (!ownership) reasons.Add("fleet.vehicle.ownership-unverified");

        if (vehicle.ProviderOrganizationId is not null) reviews.Add("fleet.vehicle.external-provider");
        return new(true, reasons.Count == 0, technical, insurance, maintenance, documents, operational, branch, ownership, reasons, reviews);
    }
}
