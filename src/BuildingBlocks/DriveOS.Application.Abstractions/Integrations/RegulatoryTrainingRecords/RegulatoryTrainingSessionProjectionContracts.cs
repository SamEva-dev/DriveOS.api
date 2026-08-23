using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;

/// <summary>
/// Provider-independent source facts captured when a training session is completed.
/// These values come from BC-10 and represent actual delivery, not the planned booking.
/// </summary>
public sealed record RegulatoryTrainingSessionProjectionSource(
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    TrainingSessionId SessionId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    BranchId? BranchId,
    Guid? VehicleId,
    string? TrainingCategory,
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    DateTimeOffset CompletedAtUtc);

public enum RegulatoryTrainingSessionProjectionStatus
{
    Complete = 1,
    Incomplete = 2,
    NotApplicable = 3
}

/// <summary>
/// A normalized missing/invalid prerequisite. Codes are stable integration diagnostics,
/// while presentation remains the responsibility of the UI/application layer.
/// </summary>
public sealed record RegulatoryTrainingSessionProjectionIssue(string Code, string? Detail = null);

/// <summary>
/// Immutable normalized regulatory view of an effectively delivered training session.
/// It deliberately contains no DSR/RdvPermis transport DTOs.
/// </summary>
public sealed record RegulatoryTrainingSessionProjection(
    Guid ProjectionId,
    int SchemaVersion,
    RegulatoryTrainingSessionProjectionStatus Status,
    string CountryCode,
    string ProviderCode,
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    TrainingSessionId SessionId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    BranchId? BranchId,
    Guid? VehicleId,
    string? TrainingCategory,
    string CurriculumCode,
    int CurriculumVersion,
    string LicenseCategoryCode,
    string TrainingMode,
    string? StudentNationalIdentifierType,
    string? StudentNationalIdentifier,
    bool StudentNationalIdentifierVerified,
    string? InstructorCredentialType,
    string? InstructorCredentialIdentifier,
    string? InstructorCredentialJurisdictionCode,
    bool InstructorCredentialVerified,
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<RegulatoryTrainingSessionProjectionIssue> Issues);

public interface IRegulatoryTrainingSessionProjector
{
    Task<Result<RegulatoryTrainingSessionProjection>> ProjectAsync(
        RegulatoryTrainingSessionProjectionSource source,
        CancellationToken cancellationToken = default);
}
