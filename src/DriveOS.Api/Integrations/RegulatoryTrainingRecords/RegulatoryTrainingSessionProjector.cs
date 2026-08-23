using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.Organizations.Application.InstructorRegulatoryCredentials;
using DriveOS.Modules.Students.Application.RegulatoryIdentities;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.RegulatoryTrainingRecords;

/// <summary>
/// Anti-corruption projector that composes authoritative facts from Students,
/// Curriculum/Pedagogy, Training Delivery and the temporary instructor credential bridge.
/// It produces a provider-independent immutable snapshot; no external API is called here.
/// </summary>
internal sealed class RegulatoryTrainingSessionProjector(
    ITrainingPathReadService trainingPaths,
    IStudentRegulatoryIdentityReadService studentIdentities,
    IInstructorRegulatoryCredentialReadService instructorCredentials)
    : IRegulatoryTrainingSessionProjector
{
    private const int CurrentSchemaVersion = 1;
    private const string France = "FR";
    private const string FrenchProvider = "fr-livret-numerique";
    private const string Neph = "NEPH";
    private const string TeachingAuthorization = "TEACHING_AUTHORIZATION";

    public async Task<Result<RegulatoryTrainingSessionProjection>> ProjectAsync(
        RegulatoryTrainingSessionProjectionSource source,
        CancellationToken cancellationToken = default)
    {
        TrainingPathDetailResponse? path = await trainingPaths.GetAsync(
            source.StudentOwnerOrganizationId,
            source.TrainingPathId,
            cancellationToken);

        if (path is null)
        {
            return Result.Failure<RegulatoryTrainingSessionProjection>(Error.NotFound(
                "RegulatoryTrainingRecord.TrainingPath.NotFound",
                "errors.regulatoryTrainingRecord.trainingPath.notFound"));
        }

        string countryCode = path.CountryCode.Trim().ToUpperInvariant();
        string providerCode = ResolveProvider(countryCode);
        var issues = new List<RegulatoryTrainingSessionProjectionIssue>();

        StudentRegulatoryIdentifierSnapshot? studentIdentifier = null;
        InstructorRegulatoryCredentialSnapshot? instructorCredential = null;

        if (countryCode == France)
        {
            studentIdentifier = await studentIdentities.ResolveCurrentAsync(
                source.StudentOwnerOrganizationId,
                source.StudentId,
                France,
                Neph,
                cancellationToken);

            instructorCredential = await instructorCredentials.ResolveCurrentAsync(
                source.PerformingOrganizationId,
                source.InstructorId,
                France,
                TeachingAuthorization,
                cancellationToken);

            if (studentIdentifier is null)
                issues.Add(new("student-national-identifier-missing", Neph));
            else if (!studentIdentifier.Verified)
                issues.Add(new("student-national-identifier-unverified", Neph));

            if (instructorCredential is null)
                issues.Add(new("instructor-credential-missing", TeachingAuthorization));
            else if (!instructorCredential.Verified)
                issues.Add(new("instructor-credential-unverified", TeachingAuthorization));

            if (instructorCredential?.ExpiresOn is DateOnly expiresOn
                && expiresOn < DateOnly.FromDateTime(source.ActualStartAtUtc.UtcDateTime))
            {
                issues.Add(new("instructor-credential-expired", expiresOn.ToString("yyyy-MM-dd")));
            }
        }

        RegulatoryTrainingSessionProjectionStatus status = string.IsNullOrWhiteSpace(providerCode)
            ? RegulatoryTrainingSessionProjectionStatus.NotApplicable
            : issues.Count == 0
                ? RegulatoryTrainingSessionProjectionStatus.Complete
                : RegulatoryTrainingSessionProjectionStatus.Incomplete;

        var projection = new RegulatoryTrainingSessionProjection(
            CreateDeterministicProjectionId(source.SessionId.Value, countryCode, CurrentSchemaVersion),
            CurrentSchemaVersion,
            status,
            countryCode,
            providerCode,
            source.OrganizationId,
            source.StudentOwnerOrganizationId,
            source.PerformingOrganizationId,
            source.SessionId,
            source.StudentId,
            source.TrainingPathId,
            source.InstructorId,
            source.BranchId,
            source.VehicleId,
            NormalizeNullable(source.TrainingCategory),
            path.CurriculumCode,
            path.CurriculumVersionNumber,
            path.LicenseCategoryCode,
            path.TrainingMode,
            studentIdentifier is null ? null : Neph,
            studentIdentifier?.Value,
            studentIdentifier?.Verified ?? false,
            instructorCredential?.CredentialType,
            instructorCredential?.Identifier,
            instructorCredential?.JurisdictionCode,
            instructorCredential?.Verified ?? false,
            source.ActualStartAtUtc.ToUniversalTime(),
            source.ActualEndAtUtc.ToUniversalTime(),
            source.DeliveredDurationMinutes,
            source.CompletedAtUtc.ToUniversalTime(),
            issues.AsReadOnly());

        return Result.Success(projection);
    }

    private static string ResolveProvider(string countryCode) =>
        countryCode == France ? FrenchProvider : string.Empty;

    private static string? NormalizeNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid CreateDeterministicProjectionId(Guid sessionId, string countryCode, int schemaVersion)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(
            $"regulatory-training-session:{sessionId:N}:{countryCode}:{schemaVersion}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}
