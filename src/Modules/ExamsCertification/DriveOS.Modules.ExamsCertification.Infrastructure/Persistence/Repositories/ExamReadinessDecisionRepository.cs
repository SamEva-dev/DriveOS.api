using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamReadinessDecisionRepository(ExamsCertificationDbContext dbContext)
    : IExamReadinessDecisionRepository
{
    public Task<ExamReadinessDecision?> GetCurrentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamReadinessDecisions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId
                    && x.StudentId == studentId
                    && x.TrainingPathId == trainingPathId
                    && x.IsCurrent,
                cancellationToken);

    public Task<ExamReadinessDecision?> GetCurrentForUpdateAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamReadinessDecisions
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId
                    && x.StudentId == studentId
                    && x.TrainingPathId == trainingPathId
                    && x.IsCurrent,
                cancellationToken);

    public void Add(ExamReadinessDecision decision) =>
        dbContext.ExamReadinessDecisions.Add(decision);
}
