using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Cancellations;

public sealed record SessionCancellationResponse(
    Guid Id, Guid OrganizationId, Guid TrainingSessionId, Guid SourceBookingId, Guid StudentOwnerOrganizationId, Guid PerformingOrganizationId,
    Guid StudentId, Guid InstructorId, Guid? VehicleId, Guid? BranchId, DateTimeOffset ActualStartAtUtc, DateTimeOffset CancelledAtUtc,
    int GrossDurationMinutes, int InterruptionDurationMinutes, int DeliveredDurationMinutes, decimal? DistanceKilometers, int Reason, string? ReasonDetails,
    int BillingDecision, int CreditDecision, decimal? PartialCreditQuantity, int ProviderCompensationDecision, string? DecisionReason,
    Guid? TrainingCreditAccountId, decimal? ReservedCreditQuantity, string? CreditReservationReference, string? PricingReference,
    Guid OperationId, Guid CancelledByUserId, DateTimeOffset CreatedAtUtc);

public interface ITrainingSessionCancellationReadService
{
    Task<SessionCancellationResponse?> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);
}

public static class SessionCancellationMappings
{
    public static SessionCancellationResponse ToResponse(SessionCancellation x) => new(
        x.Id.Value, x.OrganizationId.Value, x.TrainingSessionId.Value, x.SourceBookingId.Value, x.StudentOwnerOrganizationId.Value, x.PerformingOrganizationId.Value,
        x.StudentId.Value, x.InstructorId.Value, x.VehicleId, x.BranchId?.Value, x.ActualStartAtUtc, x.CancelledAtUtc, x.GrossDurationMinutes,
        x.InterruptionDurationMinutes, x.DeliveredDurationMinutes, x.DistanceKilometers, (int)x.Reason, x.ReasonDetails, (int)x.BillingDecision,
        (int)x.CreditDecision, x.PartialCreditQuantity, (int)x.ProviderCompensationDecision, x.DecisionReason, x.TrainingCreditAccountId?.Value,
        x.ReservedCreditQuantity, x.CreditReservationReference, x.PricingReference, x.OperationId, x.CancelledByUserId.Value, x.CreatedAtUtc);
}
