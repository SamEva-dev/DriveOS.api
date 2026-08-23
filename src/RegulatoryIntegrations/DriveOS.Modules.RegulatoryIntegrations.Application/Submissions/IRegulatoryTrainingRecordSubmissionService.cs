using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
namespace DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
public enum RegulatoryTrainingRecordReconciliationOutcome
{
    Created = 1,
    Unchanged = 2,
    Refreshed = 3,
    SupersedingRevisionCreated = 4,
    DeferredWhileProcessing = 5
}
public sealed record RegulatoryTrainingRecordReconciliationResult(
    RegulatoryTrainingRecordReconciliationOutcome Outcome,
    Guid SubmissionId,
    int Revision);
public interface IRegulatoryTrainingRecordSubmissionService
{
    Task EnsureAsync(RegulatoryTrainingSessionProjection projection, CancellationToken cancellationToken = default);
    Task<RegulatoryTrainingRecordReconciliationResult> ReconcileAsync(RegulatoryTrainingSessionProjection projection, CancellationToken cancellationToken = default);
}
