using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.RegulatoryIntegrations.Application.Persistence;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Submissions;
internal sealed class RegulatoryTrainingRecordSubmissionService(IRegulatoryTrainingRecordSubmissionRepository repository, IRegulatoryIntegrationsUnitOfWork unitOfWork, IClock clock) : IRegulatoryTrainingRecordSubmissionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task EnsureAsync(RegulatoryTrainingSessionProjection projection, CancellationToken cancellationToken = default)
        => _ = await ReconcileAsync(projection, cancellationToken);

    public async Task<RegulatoryTrainingRecordReconciliationResult> ReconcileAsync(RegulatoryTrainingSessionProjection projection, CancellationToken cancellationToken = default)
    {
        if (projection.Status == RegulatoryTrainingSessionProjectionStatus.NotApplicable || string.IsNullOrWhiteSpace(projection.ProviderCode))
            return new(RegulatoryTrainingRecordReconciliationOutcome.Unchanged, Guid.Empty, 0);

        string payload = JsonSerializer.Serialize(projection, JsonOptions);
        string issues = JsonSerializer.Serialize(projection.Issues, JsonOptions);
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        bool complete = projection.Status == RegulatoryTrainingSessionProjectionStatus.Complete;

        RegulatoryTrainingRecordSubmission? existing = await repository.GetLatestAsync(projection.ProjectionId, projection.ProviderCode, cancellationToken);
        if (existing is null)
            return await CreateAsync(projection, payload, hash, issues, complete, 1, null, RegulatoryTrainingRecordReconciliationOutcome.Created, cancellationToken);

        if (existing.HasPayloadHash(hash))
            return new(RegulatoryTrainingRecordReconciliationOutcome.Unchanged, existing.Id.Value, existing.Revision);

        if (existing.Status == RegulatoryTrainingRecordSubmissionStatus.Processing)
            return new(RegulatoryTrainingRecordReconciliationOutcome.DeferredWhileProcessing, existing.Id.Value, existing.Revision);

        if (existing.Status == RegulatoryTrainingRecordSubmissionStatus.Accepted)
        {
            var supersede = existing.MarkSuperseded();
            if (supersede.IsFailure) throw new InvalidOperationException($"Cannot supersede regulatory submission: {supersede.Error.Code}");
            return await CreateAsync(projection, payload, hash, issues, complete, existing.Revision + 1, existing.Id, RegulatoryTrainingRecordReconciliationOutcome.SupersedingRevisionCreated, cancellationToken);
        }

        if (!existing.CanRefreshSnapshot)
            return new(RegulatoryTrainingRecordReconciliationOutcome.Unchanged, existing.Id.Value, existing.Revision);

        var refresh = existing.RefreshSnapshot(payload, hash, issues, complete, clock.UtcNow);
        if (refresh.IsFailure) throw new InvalidOperationException($"Cannot refresh regulatory submission: {refresh.Error.Code}");
        await unitOfWork.CommitAsync(cancellationToken);
        return new(RegulatoryTrainingRecordReconciliationOutcome.Refreshed, existing.Id.Value, existing.Revision);
    }

    private async Task<RegulatoryTrainingRecordReconciliationResult> CreateAsync(
        RegulatoryTrainingSessionProjection projection, string payload, string hash, string issues, bool complete, int revision,
        RegulatoryTrainingRecordSubmissionId? supersedes, RegulatoryTrainingRecordReconciliationOutcome outcome, CancellationToken cancellationToken)
    {
        var result = RegulatoryTrainingRecordSubmission.Create(
            RegulatoryTrainingRecordSubmissionId.New(), projection.ProjectionId, projection.SchemaVersion, projection.OrganizationId,
            projection.StudentId, projection.TrainingPathId, projection.SessionId, projection.CountryCode, projection.ProviderCode, payload, hash, issues, complete, projection.CompletedAtUtc, revision, supersedes);
        if (result.IsFailure) throw new InvalidOperationException($"Cannot create regulatory submission: {result.Error.Code}");
        repository.Add(result.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return new(outcome, result.Value.Id.Value, result.Value.Revision);
    }
}
