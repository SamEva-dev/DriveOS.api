using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;

public enum RegulatoryTrainingRecordTransportOutcome
{
    Accepted = 1,
    Rejected = 2,
    Retry = 3,
    Unavailable = 4
}

/// <summary>
/// Immutable transport envelope handed to a country/provider adapter.
/// PayloadJson is the frozen provider-independent projection captured at submission creation.
/// </summary>
public sealed record RegulatoryTrainingRecordTransportRequest(
    Guid SubmissionId,
    Guid ProjectionId,
    int ProjectionSchemaVersion,
    OrganizationId OrganizationId,
    TrainingSessionId TrainingSessionId,
    string CountryCode,
    string ProviderCode,
    string PayloadJson,
    string PayloadHash,
    int AttemptNumber);

public sealed record RegulatoryTrainingRecordTransportResult(
    RegulatoryTrainingRecordTransportOutcome Outcome,
    string? ExternalReference = null,
    string? Code = null,
    string? Detail = null,
    TimeSpan? RetryAfter = null)
{
    public static RegulatoryTrainingRecordTransportResult Accepted(string? externalReference = null) =>
        new(RegulatoryTrainingRecordTransportOutcome.Accepted, externalReference);

    public static RegulatoryTrainingRecordTransportResult Rejected(string code, string? detail = null, string? externalReference = null) =>
        new(RegulatoryTrainingRecordTransportOutcome.Rejected, externalReference, code, detail);

    public static RegulatoryTrainingRecordTransportResult Retry(string code, string? detail = null, TimeSpan? retryAfter = null) =>
        new(RegulatoryTrainingRecordTransportOutcome.Retry, null, code, detail, retryAfter);

    public static RegulatoryTrainingRecordTransportResult Unavailable(string code, string? detail = null, TimeSpan? retryAfter = null) =>
        new(RegulatoryTrainingRecordTransportOutcome.Unavailable, null, code, detail, retryAfter);
}

/// <summary>
/// Country/provider-specific transport adapter. Implementations own authentication, DTO mapping and HTTP details.
/// </summary>
public interface IRegulatoryTrainingRecordTransportProvider
{
    string ProviderCode { get; }

    Task<RegulatoryTrainingRecordTransportResult> SendAsync(
        RegulatoryTrainingRecordTransportRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves the correct provider adapter and normalizes the "provider unavailable/not registered" case.
/// </summary>
public interface IRegulatoryTrainingRecordTransportDispatcher
{
    Task<RegulatoryTrainingRecordTransportResult> DispatchAsync(
        RegulatoryTrainingRecordTransportRequest request,
        CancellationToken cancellationToken = default);
}
