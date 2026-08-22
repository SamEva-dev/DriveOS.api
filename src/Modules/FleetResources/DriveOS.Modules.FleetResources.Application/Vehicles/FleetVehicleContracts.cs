using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FleetResources.Application.Vehicles;

public sealed record CreateFleetVehicleCommand(OrganizationId OrganizationId, VehicleId VehicleId, OrganizationId OwnerOrganizationId,
    BranchId? BranchId, string RegistrationNumber, string? Vin, string Make, string Model, string TransmissionType, string EnergyType,
    bool DualControl, IReadOnlyCollection<string> LicenseCategories, IReadOnlyCollection<string> Adaptations, UserId ActorUserId) : ICommand<VehicleId>;

public sealed record UpdateFleetVehicleComplianceCommand(OrganizationId OrganizationId, VehicleId VehicleId, bool TechnicalComplianceVerified,
    bool DocumentsCompliant, DateTimeOffset? InsuranceValidUntilUtc, bool MaintenanceBlocking, DateTimeOffset? NextMaintenanceDueAtUtc,
    VehicleOperationalStatus OperationalStatus, BranchId? BranchId, OrganizationId? ProviderOrganizationId, string? Notes, UserId ActorUserId) : ICommand;

public sealed record GetFleetVehicleQuery(OrganizationId OrganizationId, VehicleId VehicleId) : IQuery<FleetVehicleResponse>;
public sealed record GetFleetVehiclesQuery(OrganizationId OrganizationId) : IQuery<IReadOnlyList<FleetVehicleResponse>>;

public sealed record FleetVehicleResponse(Guid Id, Guid OrganizationId, Guid OwnerOrganizationId, Guid? ProviderOrganizationId, Guid? BranchId,
    string RegistrationNumber, string? Vin, string Make, string Model, string TransmissionType, string EnergyType, bool DualControl,
    IReadOnlyCollection<string> LicenseCategories, IReadOnlyCollection<string> Adaptations, string OperationalStatus,
    bool TechnicalComplianceVerified, bool DocumentsCompliant, DateTimeOffset? InsuranceValidUntilUtc, bool MaintenanceBlocking,
    DateTimeOffset? NextMaintenanceDueAtUtc, DateTimeOffset? LastComplianceVerifiedAtUtc, string? ComplianceNotes);

public sealed record FleetVehicleComplianceRequirement(string TrainingCategory, string? TransmissionType, bool DualControlRequired,
    IReadOnlyCollection<string> RequiredAdaptations, string? EnergyType);

public sealed record FleetVehicleComplianceEvaluation(bool Exists, bool IsEligible, bool TechnicalCompatibilityVerified, bool InsuranceVerified,
    bool MaintenanceVerified, bool DocumentsVerified, bool OperationalStatusVerified, bool BranchVerified, bool OwnershipVerified,
    IReadOnlyCollection<string> BlockingReasons, IReadOnlyCollection<string> Reviews);

public interface IFleetVehicleComplianceReadService
{
    Task<FleetVehicleComplianceEvaluation> EvaluateAsync(OrganizationId organizationId, VehicleId vehicleId, BranchId? branchId,
        FleetVehicleComplianceRequirement requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default);
}
