
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;

namespace DriveOS.Modules.RegulatoryIntegrations.Application.Dispatching;

public sealed record RegulatoryTrainingRecordSubmissionDispatchEnvelope(
    Guid SubmissionId,
    Guid ProjectionId,
    int ProjectionSchemaVersion,
    DriveOS.SharedKernel.Identifiers.OrganizationId OrganizationId,
    DriveOS.SharedKernel.Identifiers.TrainingSessionId TrainingSessionId,
    string CountryCode,
    string ProviderCode,
    string PayloadJson,
    string PayloadHash,
    int AttemptNumber);

public interface IRegulatoryTrainingRecordSubmissionDispatchStore
{
    Task<IReadOnlyList<RegulatoryTrainingRecordSubmissionDispatchEnvelope>> ClaimDueAsync(
        int batchSize,
        DateTimeOffset nowUtc,
        TimeSpan processingLease,
        CancellationToken cancellationToken = default);

    Task ApplyResultAsync(
        Guid submissionId,
        RegulatoryTrainingRecordTransportResult result,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset? nextAttemptAtUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        Guid submissionId,
        string code,
        string? detail,
        DateTimeOffset attemptedAtUtc,
        CancellationToken cancellationToken = default);
}
