using System.Text.Json;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.RegulatoryTrainingRecords.France;

/// <summary>
/// French anti-corruption transport adapter.
/// Resolves the tenant/branch connection, validates the normalized regulatory projection and
/// delegates only the final official protocol to IFrenchLivretNumeriqueOfficialClient.
/// </summary>
internal sealed class FrenchLivretNumeriqueTransportProvider(
    IRegulatoryIntegrationConnectionReadService connections,
    IFrenchLivretNumeriqueOfficialClient officialClient,
    ILogger<FrenchLivretNumeriqueTransportProvider> logger)
    : IRegulatoryTrainingRecordTransportProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const string Code = "fr-livret-numerique";
    public string ProviderCode => Code;

    public async Task<RegulatoryTrainingRecordTransportResult> SendAsync(
        RegulatoryTrainingRecordTransportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.CountryCode, "FR", StringComparison.OrdinalIgnoreCase))
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.country-not-supported",
                $"Country '{request.CountryCode}' is not supported by the French Livret Numérique adapter.");
        }

        RegulatoryTrainingSessionProjection? projection;
        try
        {
            projection = JsonSerializer.Deserialize<RegulatoryTrainingSessionProjection>(request.PayloadJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "Unable to deserialize regulatory projection {ProjectionId} for submission {SubmissionId}.",
                request.ProjectionId,
                request.SubmissionId);

            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.projection-invalid-json",
                "The frozen regulatory projection cannot be deserialized.");
        }

        if (projection is null)
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.projection-empty",
                "The frozen regulatory projection is empty.");
        }

        RegulatoryTrainingRecordTransportResult? validation = ValidateProjection(projection);
        if (validation is not null)
            return validation;

        RegulatoryIntegrationTransportConnectionSnapshot? connection = await connections.ResolveActiveTransportAsync(
            request.OrganizationId,
            projection.BranchId,
            "FR",
            Code,
            cancellationToken);

        if (connection is null)
        {
            return RegulatoryTrainingRecordTransportResult.Unavailable(
                "fr-livret-numerique.connection-not-configured",
                "No active Livret Numérique connection is configured for the organization/branch.",
                TimeSpan.FromHours(6));
        }

        if (string.IsNullOrWhiteSpace(connection.SecretReference))
        {
            return RegulatoryTrainingRecordTransportResult.Unavailable(
                "fr-livret-numerique.credentials-not-configured",
                "The active Livret Numérique connection does not reference transport credentials.",
                TimeSpan.FromHours(6));
        }

        var submission = new FrenchLivretNumeriqueSubmission(
            request.SubmissionId,
            request.ProjectionId,
            connection.ExternalAccountReference,
            projection.StudentNationalIdentifier!,
            projection.InstructorCredentialIdentifier!,
            projection.InstructorCredentialJurisdictionCode,
            projection.LicenseCategoryCode,
            projection.TrainingMode,
            projection.TrainingCategory,
            projection.ActualStartAtUtc,
            projection.ActualEndAtUtc,
            projection.DeliveredDurationMinutes,
            projection.VehicleId,
            request.PayloadHash);

        FrenchLivretNumeriqueOfficialClientResult result = await officialClient.SubmitAsync(
            submission,
            connection.SecretReference,
            cancellationToken);

        return result.Outcome switch
        {
            FrenchLivretNumeriqueOfficialClientOutcome.Accepted =>
                RegulatoryTrainingRecordTransportResult.Accepted(result.ExternalReference),

            FrenchLivretNumeriqueOfficialClientOutcome.Rejected =>
                RegulatoryTrainingRecordTransportResult.Rejected(
                    result.Code ?? "fr-livret-numerique.rejected",
                    result.Detail,
                    result.ExternalReference),

            FrenchLivretNumeriqueOfficialClientOutcome.Retry =>
                RegulatoryTrainingRecordTransportResult.Retry(
                    result.Code ?? "fr-livret-numerique.retry",
                    result.Detail,
                    result.RetryAfter),

            _ => RegulatoryTrainingRecordTransportResult.Unavailable(
                result.Code ?? "fr-livret-numerique.unavailable",
                result.Detail,
                result.RetryAfter)
        };
    }

    private static RegulatoryTrainingRecordTransportResult? ValidateProjection(
        RegulatoryTrainingSessionProjection projection)
    {
        if (projection.Status != RegulatoryTrainingSessionProjectionStatus.Complete)
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.projection-incomplete",
                "Only complete regulatory projections can be transported.");
        }

        if (!string.Equals(projection.StudentNationalIdentifierType, "NEPH", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(projection.StudentNationalIdentifier))
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.neph-missing",
                "A NEPH is required by the French Livret Numérique projection.");
        }

        if (!projection.StudentNationalIdentifierVerified)
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.neph-unverified",
                "The NEPH must be verified before transport.");
        }

        if (!string.Equals(projection.InstructorCredentialType, "TEACHING_AUTHORIZATION", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(projection.InstructorCredentialIdentifier))
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.instructor-authorization-missing",
                "An instructor teaching authorization is required by the French Livret Numérique projection.");
        }

        if (!projection.InstructorCredentialVerified)
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.instructor-authorization-unverified",
                "The instructor teaching authorization must be verified before transport.");
        }

        if (projection.DeliveredDurationMinutes <= 0
            || projection.ActualEndAtUtc <= projection.ActualStartAtUtc)
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.session-period-invalid",
                "The actual session period is invalid.");
        }

        if (string.IsNullOrWhiteSpace(projection.LicenseCategoryCode))
        {
            return RegulatoryTrainingRecordTransportResult.Rejected(
                "fr-livret-numerique.license-category-missing",
                "The license category is required.");
        }

        return null;
    }
}
