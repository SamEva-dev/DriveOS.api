using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;

public enum TrainingSessionCancellationConsequenceKind { TrainingCreditReconciliation = 1, BillingReconciliation = 2, ProviderCompensation = 3, VehicleUsage = 4, AnalyticsMetrics = 5, Communication = 6 }

public sealed record TrainingSessionCancellationConsequenceEnvelope(
    Guid Id, OrganizationId OrganizationId, TrainingSessionId SessionId, SessionCancellationId CancellationId,
    TrainingSessionCancellationConsequenceKind Kind, TrainingSessionConsequenceStatus Status, SessionCancellationSnapshot Snapshot, int AttemptCount, DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc, DateTimeOffset? NextAttemptAtUtc, DateTimeOffset? ProcessedAtUtc, string? LastErrorCode, string? LastErrorDetail);

public sealed record SessionCancellationSnapshot(
    OrganizationId OrganizationId, SessionCancellationId CancellationId, TrainingSessionId SessionId, BookingId SourceBookingId,
    OrganizationId StudentOwnerOrganizationId, OrganizationId PerformingOrganizationId, PersonId StudentId, UserId InstructorId, Guid? VehicleId,
    DateTimeOffset ActualStartAtUtc, DateTimeOffset CancelledAtUtc, int DeliveredDurationMinutes, decimal? DistanceKilometers,
    SessionCancellationBillingDecision BillingDecision, SessionCancellationCreditDecision CreditDecision, decimal? PartialCreditQuantity,
    SessionCancellationProviderCompensationDecision ProviderCompensationDecision, TrainingCreditAccountId? TrainingCreditAccountId, decimal? ReservedCreditQuantity,
    string? CreditReservationReference, string? PricingReference, UserId CancelledByUserId);

public interface ITrainingSessionCancellationConsequenceStore
{
    Task EnqueueAsync(SessionCancellation cancellation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingSessionCancellationConsequenceEnvelope>> ClaimDueAsync(
        int maxCount,
        DateTimeOffset now,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingSessionCancellationConsequenceEnvelope>> GetBySessionAsync(OrganizationId organizationId, TrainingSessionId sessionId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task MarkDeferredAsync(
        Guid id,
        string code,
        string? detail,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset nextAttemptAtUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid id,
        string code,
        string? detail,
        bool permanent,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset? nextAttemptAtUtc,
        CancellationToken cancellationToken = default);
    Task RequeueAsync(OrganizationId organizationId, TrainingSessionId sessionId, DateTimeOffset now, CancellationToken cancellationToken = default);
}

public interface ITrainingSessionCancellationConsequenceGateway
{
    Task<TrainingSessionConsequenceDispatchResult> DispatchAsync(TrainingSessionCancellationConsequenceEnvelope consequence, CancellationToken cancellationToken = default);
}
