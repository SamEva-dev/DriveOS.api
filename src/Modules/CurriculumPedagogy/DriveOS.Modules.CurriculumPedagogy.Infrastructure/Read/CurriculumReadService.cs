using DriveOS.Modules.CurriculumPedagogy.Application.Curricula;
using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class CurriculumReadService(Persistence.CurriculumPedagogyDbContext db) : ICurriculumReadService
{
    public async Task<IReadOnlyCollection<LicenseCategoryListItem>> ListLicenseCategoriesAsync(
        OrganizationId org,
        CancellationToken ct = default) =>
        await db.LicenseCategories
            .AsNoTracking()
            .Where(x => x.OrganizationId == org)
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.Code)
            .Select(x => new LicenseCategoryListItem(
                x.Id.Value,
                x.CountryCode,
                x.Code,
                x.Name,
                x.Status.ToString()))
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<CurriculumListItem>> ListAsync(
        OrganizationId org,
        CancellationToken ct = default) =>
        await db.Curricula
            .AsNoTracking()
            .Where(x => x.OrganizationId == org)
            .OrderBy(x => x.Code)
            .Select(x => new CurriculumListItem(
                x.Id.Value,
                x.Code,
                x.Name,
                x.CountryCode,
                x.LicenseCategoryCode,
                x.Status.ToString(),
                db.Set<CurriculumVersion>()
                    .Where(v => v.CurriculumId == x.Id)
                    .Select(v => (int?)v.VersionNumber)
                    .Max() ?? 0))
            .ToListAsync(ct);

    public async Task<CurriculumDetailResponse?> GetAsync(
        OrganizationId org,
        CurriculumId id,
        CancellationToken ct = default)
    {
        var x = await db.Curricula
            .AsNoTracking()
            .Include(x => x.Versions)
                .ThenInclude(x => x.Modules)
                    .ThenInclude(x => x.Competencies)
            .SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);

        if (x is null)
            return null;

        return new CurriculumDetailResponse(
            x.Id.Value,
            x.Code,
            x.Name,
            x.Description,
            x.CountryCode,
            x.LicenseCategoryCode,
            x.Status.ToString(),
            x.Versions
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new CurriculumVersionResponse(
                    v.Id.Value,
                    v.VersionNumber,
                    v.SourceVersionId?.Value,
                    v.Status.ToString(),
                    v.EffectiveFrom,
                    v.EffectiveTo,
                    v.ChangeSummary,
                    v.CreatedAtUtc,
                    v.PublishedAtUtc,
                    v.Modules
                        .OrderBy(m => m.Order)
                        .Select(m => new CurriculumModuleResponse(
                            m.Id.Value,
                            m.Code,
                            m.Name,
                            m.Description,
                            m.Order,
                            m.Competencies
                                .OrderBy(c => c.Order)
                                .Select(c => new CompetencyResponse(
                                    c.Id.Value,
                                    c.Code,
                                    c.Name,
                                    c.Description,
                                    c.LearningObjective,
                                    c.Order,
                                    c.IsRequired))
                                .ToArray()))
                        .ToArray()))
                .ToArray());
    }
}
