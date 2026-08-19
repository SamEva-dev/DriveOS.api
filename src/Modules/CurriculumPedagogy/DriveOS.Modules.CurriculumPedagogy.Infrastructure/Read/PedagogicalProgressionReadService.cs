using DriveOS.Modules.CurriculumPedagogy.Application.Progression;
using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class PedagogicalProgressionReadService(
    Persistence.CurriculumPedagogyDbContext db) : IPedagogicalProgressionReadService
{
    public async Task<PedagogicalProgressionOverviewResponse?> GetOverviewAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        int recentTimelineLimit = 50,
        CancellationToken cancellationToken = default)
    {
        var path = await db.TrainingPaths.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == trainingPathId, cancellationToken);
        if (path is null)
            return null;

        CurriculumVersionSnapshot? curriculum = await LoadCurriculumVersionAsync(
            organizationId, path.CurriculumVersionId, cancellationToken);
        if (curriculum is null)
            return null;

        var records = await db.CompetencyRecords.AsNoTracking()
            .Include(x => x.Assessments)
            .Where(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId)
            .ToListAsync(cancellationToken);

        var recordsByCompetency = records.ToDictionary(x => x.CompetencyId);
        var competencyRows = BuildCompetencyRows(curriculum, recordsByCompetency);
        var history = BuildTimeline(curriculum, recordsByCompetency, includeInternalComments);

        int total = competencyRows.Count;
        int required = competencyRows.Count(x => x.IsRequired);
        int assessed = competencyRows.Count(x => x.AssessmentCount > 0);
        int assessedRequired = competencyRows.Count(x => x.IsRequired && x.AssessmentCount > 0);
        int assessmentCount = records.Sum(x => x.Assessments.Count);
        int levelChangeCount = competencyRows.Sum(x => x.LevelChangeCount);

        var modules = curriculum.Modules
            .OrderBy(x => x.Order)
            .Select(module => BuildModule(module, competencyRows))
            .ToArray();

        DateTimeOffset? first = records.SelectMany(x => x.Assessments).Select(x => (DateTimeOffset?)x.AssessedAtUtc).Min();
        DateTimeOffset? last = records.SelectMany(x => x.Assessments).Select(x => (DateTimeOffset?)x.AssessedAtUtc).Max();

        return new PedagogicalProgressionOverviewResponse(
            path.Id.Value,
            path.StudentId.Value,
            path.CurriculumVersionId.Value,
            path.Status.ToString(),
            curriculum.Modules.Count,
            total,
            required,
            assessed,
            assessedRequired,
            total - assessed,
            assessmentCount,
            levelChangeCount,
            Percent(assessed, total),
            Percent(assessedRequired, required),
            first,
            last,
            Distribution(competencyRows),
            modules,
            competencyRows,
            history.Take(Math.Clamp(recentTimelineLimit, 1, 200)).ToArray());
    }

    public async Task<IReadOnlyCollection<PedagogicalProgressionTimelineItemResponse>?> GetHistoryAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        bool includeInternalComments,
        CancellationToken cancellationToken = default)
    {
        var path = await db.TrainingPaths.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == trainingPathId, cancellationToken);
        if (path is null)
            return null;

        CurriculumVersionSnapshot? curriculum = await LoadCurriculumVersionAsync(
            organizationId, path.CurriculumVersionId, cancellationToken);
        if (curriculum is null)
            return null;

        var records = await db.CompetencyRecords.AsNoTracking()
            .Include(x => x.Assessments)
            .Where(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId)
            .ToListAsync(cancellationToken);

        return BuildTimeline(curriculum, records.ToDictionary(x => x.CompetencyId), includeInternalComments);
    }

    private async Task<CurriculumVersionSnapshot?> LoadCurriculumVersionAsync(
        OrganizationId organizationId,
        CurriculumVersionId curriculumVersionId,
        CancellationToken cancellationToken)
    {
        var curricula = await db.Curricula.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Include(x => x.Versions)
                .ThenInclude(x => x.Modules)
                    .ThenInclude(x => x.Competencies)
            .ToListAsync(cancellationToken);

        foreach (var curriculum in curricula)
        {
            CurriculumVersion? version = curriculum.Versions.SingleOrDefault(x => x.Id == curriculumVersionId);
            if (version is null)
                continue;

            return new CurriculumVersionSnapshot(
                version.Id,
                version.Modules.OrderBy(x => x.Order)
                    .Select(module => new ModuleSnapshot(
                        module.Id,
                        module.Code,
                        module.Name,
                        module.Order,
                        module.Competencies.OrderBy(x => x.Order)
                            .Select(competency => new CompetencySnapshot(
                                competency.Id,
                                competency.Code,
                                competency.Name,
                                competency.IsRequired,
                                competency.Order))
                            .ToArray()))
                    .ToArray());
        }

        return null;
    }

    private static IReadOnlyCollection<PedagogicalProgressionCompetencyResponse> BuildCompetencyRows(
        CurriculumVersionSnapshot curriculum,
        IReadOnlyDictionary<CompetencyId, CompetencyRecord> records)
    {
        var result = new List<PedagogicalProgressionCompetencyResponse>();

        foreach (ModuleSnapshot module in curriculum.Modules.OrderBy(x => x.Order))
        {
            foreach (CompetencySnapshot competency in module.Competencies.OrderBy(x => x.Order))
            {
                records.TryGetValue(competency.Id, out CompetencyRecord? record);
                var ordered = record?.Assessments
                    .OrderBy(x => x.AssessedAtUtc)
                    .ThenBy(x => x.RecordedAtUtc)
                    .ToArray() ?? [];

                int levelChanges = 0;
                string? previous = null;
                foreach (CompetencyAssessment assessment in ordered)
                {
                    if (previous is not null && !string.Equals(previous, assessment.LevelCode, StringComparison.OrdinalIgnoreCase))
                        levelChanges++;
                    previous = assessment.LevelCode;
                }

                result.Add(new PedagogicalProgressionCompetencyResponse(
                    competency.Id.Value,
                    module.Id.Value,
                    module.Code,
                    competency.Code,
                    competency.Name,
                    competency.IsRequired,
                    competency.Order,
                    record?.CurrentLevelCode,
                    record?.LastAssessedAtUtc,
                    record?.LastAssessorUserId?.Value,
                    ordered.Length,
                    levelChanges));
            }
        }

        return result;
    }

    private static PedagogicalProgressionModuleResponse BuildModule(
        ModuleSnapshot module,
        IReadOnlyCollection<PedagogicalProgressionCompetencyResponse> allCompetencies)
    {
        PedagogicalProgressionCompetencyResponse[] competencies = allCompetencies
            .Where(x => x.ModuleId == module.Id.Value)
            .ToArray();
        int total = competencies.Length;
        int required = competencies.Count(x => x.IsRequired);
        int assessed = competencies.Count(x => x.AssessmentCount > 0);
        int assessedRequired = competencies.Count(x => x.IsRequired && x.AssessmentCount > 0);

        return new PedagogicalProgressionModuleResponse(
            module.Id.Value,
            module.Code,
            module.Name,
            module.Order,
            total,
            required,
            assessed,
            total - assessed,
            competencies.Sum(x => x.AssessmentCount),
            Percent(assessed, total),
            Percent(assessedRequired, required),
            Distribution(competencies));
    }

    private static IReadOnlyCollection<PedagogicalProgressionLevelDistributionResponse> Distribution(
        IEnumerable<PedagogicalProgressionCompetencyResponse> competencies)
    {
        PedagogicalProgressionCompetencyResponse[] assessed = competencies
            .Where(x => !string.IsNullOrWhiteSpace(x.CurrentLevelCode))
            .ToArray();

        if (assessed.Length == 0)
            return Array.Empty<PedagogicalProgressionLevelDistributionResponse>();

        return assessed
            .GroupBy(x => x.CurrentLevelCode!, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PedagogicalProgressionLevelDistributionResponse(
                group.Key,
                group.Count(),
                Percent(group.Count(), assessed.Length)))
            .ToArray();
    }

    private static IReadOnlyCollection<PedagogicalProgressionTimelineItemResponse> BuildTimeline(
        CurriculumVersionSnapshot curriculum,
        IReadOnlyDictionary<CompetencyId, CompetencyRecord> records,
        bool includeInternalComments)
    {
        var metadata = curriculum.Modules
            .SelectMany(module => module.Competencies.Select(competency => new
            {
                competency.Id,
                Competency = competency,
                Module = module
            }))
            .ToDictionary(x => x.Id);

        var timeline = new List<PedagogicalProgressionTimelineItemResponse>();

        foreach ((CompetencyId competencyId, CompetencyRecord record) in records)
        {
            if (!metadata.TryGetValue(competencyId, out var item))
                continue;

            CompetencyAssessment[] ordered = record.Assessments
                .OrderBy(x => x.AssessedAtUtc)
                .ThenBy(x => x.RecordedAtUtc)
                .ToArray();

            string? previousLevel = null;
            foreach (CompetencyAssessment assessment in ordered)
            {
                bool changed = previousLevel is not null &&
                    !string.Equals(previousLevel, assessment.LevelCode, StringComparison.OrdinalIgnoreCase);

                timeline.Add(new PedagogicalProgressionTimelineItemResponse(
                    assessment.Id.Value,
                    competencyId.Value,
                    item.Competency.Code,
                    item.Competency.Name,
                    item.Module.Id.Value,
                    item.Module.Code,
                    item.Module.Name,
                    assessment.LevelCode,
                    previousLevel,
                    changed,
                    assessment.AssessorUserId.Value,
                    assessment.SourceSessionId,
                    includeInternalComments || assessment.IsVisibleToStudent ? assessment.Comment : null,
                    assessment.IsVisibleToStudent,
                    assessment.AssessedAtUtc,
                    assessment.RecordedAtUtc));

                previousLevel = assessment.LevelCode;
            }
        }

        return timeline
            .OrderByDescending(x => x.AssessedAtUtc)
            .ThenByDescending(x => x.RecordedAtUtc)
            .ToArray();
    }

    private static decimal Percent(int value, int total) =>
        total <= 0 ? 0m : Math.Round(value * 100m / total, 1, MidpointRounding.AwayFromZero);

    private sealed record CurriculumVersionSnapshot(
        CurriculumVersionId Id,
        IReadOnlyCollection<ModuleSnapshot> Modules);

    private sealed record ModuleSnapshot(
        CurriculumModuleId Id,
        string Code,
        string Name,
        int Order,
        IReadOnlyCollection<CompetencySnapshot> Competencies);

    private sealed record CompetencySnapshot(
        CompetencyId Id,
        string Code,
        string Name,
        bool IsRequired,
        int Order);
}
