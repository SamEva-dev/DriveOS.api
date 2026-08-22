using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamReadinessOpinionRepository(ExamsCertificationDbContext dbContext) : IExamReadinessOpinionRepository
{
    public Task<ExamReadinessOpinion?> GetByOperationIdAsync(
        OrganizationId organizationId,
        Guid operationId,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamReadinessOpinions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.OperationId == operationId, cancellationToken);

    public Task<ExamReadinessOpinion?> GetLatestByAuthorAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        UserId authorId,
        CancellationToken cancellationToken = default) =>
        dbContext.ExamReadinessOpinions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.StudentId == studentId
                && x.TrainingPathId == trainingPathId
                && x.AuthorId == authorId)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ExamReadinessOpinion>> ListAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default) =>
        await dbContext.ExamReadinessOpinions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.StudentId == studentId && x.TrainingPathId == trainingPathId)
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

    public void Add(ExamReadinessOpinion opinion) => dbContext.ExamReadinessOpinions.Add(opinion);
}
