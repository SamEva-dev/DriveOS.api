namespace DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;

public interface IRegulatoryTrainingRecordSubmissionRepository
{
    Task<RegulatoryTrainingRecordSubmission?> GetLatestAsync(Guid projectionId, string providerCode, CancellationToken cancellationToken = default);
    void Add(RegulatoryTrainingRecordSubmission submission);
}
