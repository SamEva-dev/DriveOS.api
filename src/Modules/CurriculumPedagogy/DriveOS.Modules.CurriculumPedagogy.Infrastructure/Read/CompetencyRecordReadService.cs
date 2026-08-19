using DriveOS.Modules.CurriculumPedagogy.Application.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class CompetencyRecordReadService(Persistence.CurriculumPedagogyDbContext db) : ICompetencyRecordReadService
{
    public async Task<IReadOnlyCollection<CompetencyRecordResponse>> ListForTrainingPathAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default)
    {
        var path = await db.TrainingPaths.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == trainingPathId, cancellationToken);
        if (path is null) return Array.Empty<CompetencyRecordResponse>();

        IReadOnlyCollection<CompetencyMetadata> competencies = await LoadCompetenciesAsync(
            organizationId, path.CurriculumVersionId, cancellationToken);

        var records = await db.CompetencyRecords.AsNoTracking().Include(x => x.Assessments)
            .Where(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId)
            .ToListAsync(cancellationToken);
        var byCompetency = records.ToDictionary(x => x.CompetencyId);

        return competencies
            .OrderBy(x => x.ModuleOrder).ThenBy(x => x.CompetencyOrder)
            .Select(x => byCompetency.TryGetValue(x.Id, out CompetencyRecord? record)
                ? Map(record, x, includeInternalComments)
                : Empty(trainingPathId, path.CurriculumVersionId, x))
            .ToArray();
    }

    public async Task<CompetencyRecordResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CompetencyId competencyId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default)
    {
        var path = await db.TrainingPaths.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == trainingPathId, cancellationToken);
        if (path is null) return null;

        CompetencyMetadata? metadata = (await LoadCompetenciesAsync(organizationId, path.CurriculumVersionId, cancellationToken))
            .SingleOrDefault(x => x.Id == competencyId);
        if (metadata is null) return null;

        CompetencyRecord? record = await db.CompetencyRecords.AsNoTracking().Include(x => x.Assessments)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId && x.CompetencyId == competencyId, cancellationToken);

        return record is null
            ? Empty(trainingPathId, path.CurriculumVersionId, metadata)
            : Map(record, metadata, includeInternalComments);
    }

    private async Task<IReadOnlyCollection<CompetencyMetadata>> LoadCompetenciesAsync(
        OrganizationId organizationId,
        CurriculumVersionId curriculumVersionId,
        CancellationToken cancellationToken)
    {
        var curricula = await db.Curricula.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Include(x => x.Versions).ThenInclude(x => x.Modules).ThenInclude(x => x.Competencies)
            .ToListAsync(cancellationToken);

        return curricula.SelectMany(x => x.Versions)
            .Where(x => x.Id == curriculumVersionId)
            .SelectMany(x => x.Modules.SelectMany(m => m.Competencies.Select(c => new CompetencyMetadata(
                c.Id, c.Code, c.Name, c.IsRequired, m.Order, c.Order))))
            .ToArray();
    }

    private static CompetencyRecordResponse Empty(
        TrainingPathId trainingPathId,
        CurriculumVersionId curriculumVersionId,
        CompetencyMetadata metadata) =>
        new(null, trainingPathId.Value, curriculumVersionId.Value, metadata.Id.Value, metadata.Code, metadata.Name,
            metadata.IsRequired, null, null, null, Array.Empty<CompetencyAssessmentResponse>());

    private static CompetencyRecordResponse Map(
        CompetencyRecord record,
        CompetencyMetadata metadata,
        bool includeInternalComments)
    {
        var assessments = record.Assessments
            .Where(x => includeInternalComments || x.IsVisibleToStudent)
            .OrderByDescending(x => x.AssessedAtUtc).ThenByDescending(x => x.RecordedAtUtc)
            .Select(x => new CompetencyAssessmentResponse(
                x.Id.Value, x.LevelCode, x.AssessorUserId.Value, x.SourceSessionId, x.Comment,
                x.IsVisibleToStudent, x.AssessedAtUtc, x.RecordedAtUtc))
            .ToArray();

        return new(record.Id.Value, record.TrainingPathId.Value, record.CurriculumVersionId.Value,
            record.CompetencyId.Value, metadata.Code, metadata.Name, record.IsRequired,
            record.CurrentLevelCode, record.LastAssessedAtUtc, record.LastAssessorUserId?.Value, assessments);
    }

    private sealed record CompetencyMetadata(
        CompetencyId Id,
        string Code,
        string Name,
        bool IsRequired,
        int ModuleOrder,
        int CompetencyOrder);
}
