namespace DriveOS.Api.Integrations.RegulatoryTrainingRecords.France;

/// <summary>
/// Provider-semantic representation derived from the DriveOS regulatory projection.
/// This is intentionally NOT a DSR wire DTO: field names, endpoint shapes and authentication
/// must be implemented only from the official editor contract.
/// </summary>
internal sealed record FrenchLivretNumeriqueSubmission(
    Guid SubmissionId,
    Guid ProjectionId,
    string ExternalAccountReference,
    string Neph,
    string InstructorAuthorizationNumber,
    string? InstructorAuthorizationJurisdiction,
    string LicenseCategoryCode,
    string TrainingMode,
    string? TrainingCategory,
    DateTimeOffset ActualStartAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int DeliveredDurationMinutes,
    Guid? VehicleId,
    string PayloadHash);

internal enum FrenchLivretNumeriqueOfficialClientOutcome
{
    Accepted = 1,
    Rejected = 2,
    Retry = 3,
    Unavailable = 4
}

internal sealed record FrenchLivretNumeriqueOfficialClientResult(
    FrenchLivretNumeriqueOfficialClientOutcome Outcome,
    string? ExternalReference = null,
    string? Code = null,
    string? Detail = null,
    TimeSpan? RetryAfter = null);

/// <summary>
/// Last boundary before the official DSR/RdvPermis editor protocol.
/// The implementation remains unavailable until authoritative endpoint/authentication/wire specs
/// and homologation credentials are supplied by the administration.
/// </summary>
internal interface IFrenchLivretNumeriqueOfficialClient
{
    Task<FrenchLivretNumeriqueOfficialClientResult> SubmitAsync(
        FrenchLivretNumeriqueSubmission submission,
        string secretReference,
        CancellationToken cancellationToken = default);
}
