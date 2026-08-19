using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class TrainingPathReadService(CurriculumPedagogyDbContext db) : ITrainingPathReadService
{
    public async Task<IReadOnlyCollection<TrainingPathListItem>> ListForStudentAsync(
        OrganizationId organizationId,
        PersonId studentId,
        CancellationToken ct = default)
    {
        var paths = await db.TrainingPaths.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
        if (paths.Count == 0) return [];

        var curriculaGraph = await db.Curricula.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .Include(c => c.Versions)
            .ToListAsync(ct);
        var byVersion = curriculaGraph
            .SelectMany(c => c.Versions.Select(v => new
            {
                VersionId = v.Id,
                c.Code,
                c.Name,
                v.VersionNumber,
                v.CountryCodeSnapshot,
                v.LicenseCategoryCodeSnapshot
            }))
            .ToDictionary(x => x.VersionId);

        return paths.Select(x =>
        {
            byVersion.TryGetValue(x.CurriculumVersionId, out var c);
            return new TrainingPathListItem(
                x.Id.Value, x.StudentId.Value, x.CurriculumVersionId.Value,
                c?.Code ?? string.Empty, c?.Name ?? string.Empty, c?.VersionNumber ?? 0,
                c?.CountryCodeSnapshot ?? string.Empty, c?.LicenseCategoryCodeSnapshot ?? string.Empty,
                x.TrainingMode.ToString(), x.StartDate, x.TargetCompletionDate,
                x.EstimatedPracticalHours, x.Status.ToString(), x.CreatedAtUtc);
        }).ToArray();
    }

    public async Task<TrainingPathDetailResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CancellationToken ct = default)
    {
        var path = await db.TrainingPaths.AsNoTracking().Include(x => x.Milestones)
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == trainingPathId, ct);
        if (path is null) return null;

        var curricula = await db.Curricula.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Include(x => x.Versions)
            .ToListAsync(ct);
        var c = curricula
            .SelectMany(x => x.Versions.Where(v => v.Id == path.CurriculumVersionId), (x, v) => new
            {
                CurriculumId = x.Id,
                x.Code,
                x.Name,
                v.VersionNumber,
                v.CountryCodeSnapshot,
                v.LicenseCategoryCodeSnapshot
            }).SingleOrDefault();
        if (c is null) return null;

        return new TrainingPathDetailResponse(
            path.Id.Value, path.StudentId.Value, path.CurriculumVersionId.Value, c.CurriculumId.Value,
            c.Code, c.Name, c.VersionNumber, c.CountryCodeSnapshot, c.LicenseCategoryCodeSnapshot,
            path.TrainingMode.ToString(), path.StartDate, path.TargetCompletionDate,
            path.EstimatedPracticalHours, path.Status.ToString(), path.ActivatedAtUtc,
            path.CompletedAtUtc, path.SuspendedAtUtc, path.SuspensionReason,
            path.CancelledAtUtc, path.CancellationReason, path.CreatedAtUtc,
            path.Milestones.OrderBy(x => x.Order).Select(x => new TrainingPathMilestoneResponse(
                x.Id.Value, x.Code, x.Name, x.Description, x.Order, x.TargetDate,
                x.Status.ToString(), x.CompletedAtUtc)).ToArray());
    }
}
