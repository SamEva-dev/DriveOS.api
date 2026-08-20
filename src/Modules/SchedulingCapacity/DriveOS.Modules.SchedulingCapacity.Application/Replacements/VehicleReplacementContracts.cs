using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Replacements;

public sealed record VehicleReplacementRequirements(string TrainingCategory, string? TransmissionType, bool DualControlRequired,
    IReadOnlyCollection<string> RequiredAdaptations, string? EnergyType);

public sealed record VehicleReplacementEligibility(bool IsEligible, bool TechnicalCompatibilityVerified, bool InsuranceVerified,
    bool MaintenanceVerified, bool LocationVerified, bool OwnershipVerified, IReadOnlyCollection<string> BlockingReasons, IReadOnlyCollection<string> ExternalReviews);

public interface IVehicleReplacementEligibilityGateway
{
    Task<VehicleReplacementEligibility> EvaluateAsync(OrganizationId organizationId, Guid vehicleId, BranchId? branchId,
        VehicleReplacementRequirements requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default);
}

public sealed record VehicleReplacementSuggestionResponse(Guid VehicleId, Guid CalendarResourceId, string DisplayName, Guid? BranchId,
    bool IsAvailableForAllBookings, bool TechnicalCompatibilityVerified, bool InsuranceVerified, bool MaintenanceVerified, bool LocationVerified, bool OwnershipVerified,
    int CompatibleBookingCount, int TargetBookingCount, int Score, IReadOnlyCollection<string> Factors,
    IReadOnlyCollection<string> BlockingReasons, IReadOnlyCollection<string> ExternalReviews);

public sealed record VehicleReplacementPreviewResponse(Guid OperationId, Guid PreviousVehicleId, Guid ReplacementVehicleId, int Mode,
    IReadOnlyCollection<Guid> BookingIds, bool CanConfirm, IReadOnlyCollection<string> BlockingReasons, IReadOnlyCollection<string> ExternalReviews);

public sealed record VehicleReplacementApplyResponse(Guid OperationId, int ReplacedBookingCount, IReadOnlyCollection<Guid> BookingIds);

public interface IVehicleReplacementService
{
    Task<IReadOnlyCollection<VehicleReplacementSuggestionResponse>> SuggestAsync(OrganizationId organizationId, Guid previousVehicleId,
        IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, CancellationToken cancellationToken = default);
    Task<VehicleReplacementPreviewResponse?> PreviewAsync(OrganizationId organizationId, Guid operationId, Guid previousVehicleId, Guid replacementVehicleId,
        int mode, IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, CancellationToken cancellationToken = default);
    Task<Result<VehicleReplacementApplyResponse>> ApplyAsync(OrganizationId organizationId, Guid operationId, Guid previousVehicleId, Guid replacementVehicleId,
        int mode, IReadOnlyCollection<BookingId> bookingIds, VehicleReplacementRequirements requirements, string reason, CancellationToken cancellationToken = default);
}
