using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.TrainingDelivery;

/// <summary>
/// Conservative bridge used until BC-14 Fleet et Resources becomes the authoritative vehicle-compliance source.
/// A vehicle-based training session is intentionally blocked rather than being declared compliant from Scheduling-only data.
/// </summary>
internal sealed class TrainingSessionVehicleComplianceGateway : ITrainingSessionVehicleComplianceGateway
{
    public Task<TrainingSessionVehicleCompliance> CheckAsync(
        OrganizationId organizationId,
        Guid vehicleId,
        BranchId? branchId,
        string? trainingCategory,
        DateTimeOffset plannedStartAtUtc,
        DateTimeOffset plannedEndAtUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new TrainingSessionVehicleCompliance(
            false,
            false,
            ["fleet.vehicle.authoritative-data-unavailable"],
            ["fleet.vehicle.compliance-integration-required"]));
}
