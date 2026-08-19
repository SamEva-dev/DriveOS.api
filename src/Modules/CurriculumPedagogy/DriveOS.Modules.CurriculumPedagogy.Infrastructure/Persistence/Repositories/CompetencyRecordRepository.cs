using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Repositories;

internal sealed class CompetencyRecordRepository(CurriculumPedagogyDbContext db) : ICompetencyRecordRepository
{
    private IQueryable<CompetencyRecord> Graph => db.CompetencyRecords.Include(x => x.Assessments);

    public Task<CompetencyRecord?> GetByIdAsync(OrganizationId organizationId, CompetencyRecordId competencyRecordId, CancellationToken cancellationToken = default) =>
        Graph.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == competencyRecordId, cancellationToken);

    public Task<CompetencyRecord?> GetByIdForUpdateAsync(OrganizationId organizationId, CompetencyRecordId competencyRecordId, CancellationToken cancellationToken = default) =>
        Graph.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == competencyRecordId, cancellationToken);

    public Task<CompetencyRecord?> GetByTrainingPathAndCompetencyAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CompetencyId competencyId, CancellationToken cancellationToken = default) =>
        Graph.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId && x.CompetencyId == competencyId, cancellationToken);

    public Task<CompetencyRecord?> GetByTrainingPathAndCompetencyForUpdateAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CompetencyId competencyId, CancellationToken cancellationToken = default) =>
        Graph.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId && x.CompetencyId == competencyId, cancellationToken);

    public async Task AddAsync(CompetencyRecord competencyRecord, CancellationToken cancellationToken = default) =>
        await db.CompetencyRecords.AddAsync(competencyRecord, cancellationToken);
}
