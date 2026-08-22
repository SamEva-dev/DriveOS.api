using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;

public interface IExamReadinessOpinionRepository
{
    Task<ExamReadinessOpinion?> GetByOperationIdAsync(OrganizationId organizationId, Guid operationId, CancellationToken cancellationToken = default);
    Task<ExamReadinessOpinion?> GetLatestByAuthorAsync(OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, UserId authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamReadinessOpinion>> ListAsync(OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, CancellationToken cancellationToken = default);
    void Add(ExamReadinessOpinion opinion);
}
