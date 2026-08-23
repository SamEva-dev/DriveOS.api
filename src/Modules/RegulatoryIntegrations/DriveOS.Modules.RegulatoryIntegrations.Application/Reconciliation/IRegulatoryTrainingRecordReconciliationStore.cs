namespace DriveOS.Modules.RegulatoryIntegrations.Application.Reconciliation;

public sealed record RegulatoryTrainingRecordReconciliationCandidate(
    Guid SubmissionId,
    string PayloadJson);

public interface IRegulatoryTrainingRecordReconciliationStore
{
    Task<IReadOnlyList<RegulatoryTrainingRecordReconciliationCandidate>> GetCandidatesAsync(
        int batchSize, CancellationToken cancellationToken = default);
}
