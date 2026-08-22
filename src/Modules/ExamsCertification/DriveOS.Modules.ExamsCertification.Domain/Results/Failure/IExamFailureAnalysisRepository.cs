using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results.Failure;

public interface IExamFailureAnalysisRepository
{
    Task<ExamFailureAnalysis?> GetLatestByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task<ExamFailureAnalysis?> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default);
    Task<ExamFailureAnalysis?> GetByResultForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default);
    void Add(ExamFailureAnalysis analysis);
}
