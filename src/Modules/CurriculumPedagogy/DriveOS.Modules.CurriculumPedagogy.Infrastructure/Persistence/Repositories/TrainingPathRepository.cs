using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Repositories;

internal sealed class TrainingPathRepository(CurriculumPedagogyDbContext db) : ITrainingPathRepository
{
    private IQueryable<TrainingPath> Graph => db.TrainingPaths.Include(x => x.Milestones);

    public Task<TrainingPath?> GetByIdAsync(TrainingPathId trainingPathId, OrganizationId organizationId, CancellationToken ct = default) =>
        Graph.AsNoTracking().SingleOrDefaultAsync(x => x.Id == trainingPathId && x.OrganizationId == organizationId, ct);

    public Task<TrainingPath?> GetByIdForUpdateAsync(TrainingPathId trainingPathId, OrganizationId organizationId, CancellationToken ct = default) =>
        Graph.SingleOrDefaultAsync(x => x.Id == trainingPathId && x.OrganizationId == organizationId, ct);

    public Task<bool> ExistsOpenForStudentAndVersionAsync(OrganizationId organizationId, PersonId studentId, CurriculumVersionId curriculumVersionId, CancellationToken ct = default) =>
        db.TrainingPaths.AsNoTracking().AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.StudentId == studentId &&
            x.CurriculumVersionId == curriculumVersionId &&
            x.Status != TrainingPathStatus.Completed &&
            x.Status != TrainingPathStatus.Cancelled,
            ct);

    public async Task AddAsync(TrainingPath trainingPath, CancellationToken ct = default) =>
        await db.TrainingPaths.AddAsync(trainingPath, ct);
}
