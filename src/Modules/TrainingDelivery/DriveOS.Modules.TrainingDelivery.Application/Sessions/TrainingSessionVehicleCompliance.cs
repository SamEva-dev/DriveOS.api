using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record TrainingSessionVehicleCompliance(
    bool IsVerified,
    bool IsOperational,
    IReadOnlyCollection<string> BlockingReasons,
    IReadOnlyCollection<string> ExternalReviews);

public interface ITrainingSessionVehicleComplianceGateway
{
    Task<TrainingSessionVehicleCompliance> CheckAsync(
        OrganizationId organizationId,
        Guid vehicleId,
        BranchId? branchId,
        string? trainingCategory,
        DateTimeOffset plannedStartAtUtc,
        DateTimeOffset plannedEndAtUtc,
        CancellationToken cancellationToken = default);
}
