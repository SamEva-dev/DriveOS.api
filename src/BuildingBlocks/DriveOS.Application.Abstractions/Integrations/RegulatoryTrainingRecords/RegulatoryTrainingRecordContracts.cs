using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;

/// <summary>
/// Stable provider-independent status of a national or regulatory training record.
/// Country-specific adapters translate their external states into this contract.
/// </summary>
public enum RegulatoryTrainingRecordStatus
{
    NotApplicable = 0,
    Pending = 1,
    Compliant = 2,
    Warning = 3,
    Blocked = 4,
    Unavailable = 5
}

/// <summary>
/// Context required to evaluate the regulatory learning/training record of a student.
/// This contract deliberately contains no DSR/RdvPermis-specific transport model.
/// </summary>
public sealed record RegulatoryTrainingRecordContext(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    string? CountryCode,
    string ExamType,
    string LicenseCategory,
    BranchId? BranchId = null);

/// <summary>
/// Provider-independent evaluation returned to DriveOS bounded contexts.
/// Evidence must remain diagnostic metadata and must not become the UI contract.
/// </summary>
public sealed record RegulatoryTrainingRecordEvaluation(
    bool Required,
    RegulatoryTrainingRecordStatus Status,
    string ProviderCode,
    string? ExternalReference = null,
    string? Evidence = null);

/// <summary>
/// Cross-context port used by DriveOS business modules to query the authoritative
/// regulatory training record without depending on a country-specific integration.
/// </summary>
public interface IRegulatoryTrainingRecordGateway
{
    Task<Result<RegulatoryTrainingRecordEvaluation>> EvaluateAsync(
        RegulatoryTrainingRecordContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter contract implemented by country/provider integrations.
/// A provider may be discoverable while not yet configured or authorized.
/// </summary>
public interface IRegulatoryTrainingRecordProvider
{
    string ProviderCode { get; }

    bool CanHandle(string? countryCode);

    Task<Result<RegulatoryTrainingRecordEvaluation>> EvaluateAsync(
        RegulatoryTrainingRecordContext context,
        CancellationToken cancellationToken = default);
}
