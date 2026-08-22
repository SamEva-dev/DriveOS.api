using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ExamsCertification.Domain.Results.Success;
public interface IExamSuccessProcessRepository
{
    Task<ExamSuccessProcess?> GetLatestByResultAsync(OrganizationId organizationId, ExamResultId resultId, CancellationToken cancellationToken = default);
    Task<ExamSuccessProcess?> GetByResultAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default);
    Task<ExamSuccessProcess?> GetByResultForUpdateAsync(OrganizationId organizationId, ExamResultId resultId, int resultRevision, CancellationToken cancellationToken = default);
    void Add(ExamSuccessProcess process);
}
