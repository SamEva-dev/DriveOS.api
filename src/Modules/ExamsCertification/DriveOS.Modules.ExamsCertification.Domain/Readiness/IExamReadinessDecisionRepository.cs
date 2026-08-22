using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Readiness;

public interface IExamReadinessDecisionRepository
{
    Task<ExamReadinessDecision?> GetCurrentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default);

    Task<ExamReadinessDecision?> GetCurrentForUpdateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default);

    void Add(ExamReadinessDecision decision);
}
