using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Results;

public interface IExamResultRepository
{
    Task<ExamResult?> GetByIdAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task<ExamResult?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task<ExamResult?> GetByAttemptAsync(OrganizationId organizationId, ExamAttemptId attemptId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResult>> ListByStudentAsync(OrganizationId organizationId, PersonId studentId, CancellationToken cancellationToken = default);
    Task<ExamResult?> FindByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    void Add(ExamResult result);
}
