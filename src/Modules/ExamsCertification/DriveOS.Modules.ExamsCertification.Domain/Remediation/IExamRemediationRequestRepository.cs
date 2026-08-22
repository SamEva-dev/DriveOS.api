using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Remediation;

public interface IExamRemediationRequestRepository
{
    Task<ExamRemediationRequest?> GetByIdAsync(OrganizationId organizationId, ExamRemediationRequestId id, CancellationToken ct = default);
    Task<ExamRemediationRequest?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamRemediationRequestId id, CancellationToken ct = default);
    Task<ExamRemediationRequest?> GetByAnalysisAsync(OrganizationId organizationId, ExamFailureAnalysisId analysisId, CancellationToken ct = default);
    Task<ExamRemediationRequest?> GetByResultRevisionForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken ct = default);
    Task<IReadOnlyList<ExamRemediationRequest>> ListByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken ct = default);
    void Add(ExamRemediationRequest request);
}
