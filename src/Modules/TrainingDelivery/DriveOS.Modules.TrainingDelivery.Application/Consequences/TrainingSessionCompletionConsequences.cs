using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Consequences;

public enum TrainingSessionConsequenceKind
{
    TrainingCreditConsumption = 1,
    BillingServiceRecognition = 2,
    ProfessionalServiceEntry = 3,
    VehicleUsage = 4,
    AnalyticsMetrics = 5,
    SessionSummaryCommunication = 6,
    RegulatoryTrainingRecordSubmission = 7
}

public enum TrainingSessionConsequenceStatus
{
    Pending = 1,
    Processing = 2,
    Deferred = 3,
    Failed = 4,
    Processed = 5,
    DeadLetter = 6
}

public sealed record TrainingSessionCompletionSnapshot(
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    TrainingSessionId SessionId,
    BookingId SourceBookingId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    Guid? VehicleId,
    BranchId? BranchId,
    string? TrainingCategory,
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    decimal? DistanceKilometers,
    decimal? StartEnergyLevelPercent,
    decimal? EndEnergyLevelPercent,
    decimal FuelAddedLiters,
    decimal ChargedEnergyKwh,
    string? PricingReference,
    TrainingCreditAccountId? TrainingCreditAccountId,
    decimal? CreditQuantity,
    string? CreditReservationReference,
    Guid CompletionOperationId,
    UserId CompletedByUserId,
    DateTimeOffset CompletedAtUtc);

public sealed record TrainingSessionConsequenceEnvelope(
    Guid Id,
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    TrainingSessionConsequenceKind Kind,
    TrainingSessionConsequenceStatus Status,
    TrainingSessionCompletionSnapshot Snapshot,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    string? LastErrorCode,
    string? LastErrorDetail);

public sealed record TrainingSessionConsequenceDispatchResult(
    bool IsProcessed,
    bool IsDeferred,
    bool IsPermanentFailure,
    string? Code = null,
    string? Detail = null)
{
    public static TrainingSessionConsequenceDispatchResult Processed() => new(true, false, false);
    public static TrainingSessionConsequenceDispatchResult Deferred(string code, string? detail = null) => new(false, true, false, code, detail);
    public static TrainingSessionConsequenceDispatchResult Retry(string code, string? detail = null) => new(false, false, false, code, detail);
    public static TrainingSessionConsequenceDispatchResult PermanentFailure(string code, string? detail = null) => new(false, false, true, code, detail);
}

public interface ITrainingSessionCompletionConsequenceStore
{
    Task EnqueueAsync(TrainingSessionCompletionSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingSessionConsequenceEnvelope>> ClaimDueAsync(
        int maxCount,
        DateTimeOffset now,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingSessionConsequenceEnvelope>> GetBySessionAsync(OrganizationId organizationId, TrainingSessionId sessionId, CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid id, DateTimeOffset processedAtUtc, CancellationToken cancellationToken = default);
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

public interface ITrainingSessionCompletionConsequenceGateway
{
    Task<TrainingSessionConsequenceDispatchResult> DispatchAsync(TrainingSessionConsequenceEnvelope consequence, CancellationToken cancellationToken = default);
}
