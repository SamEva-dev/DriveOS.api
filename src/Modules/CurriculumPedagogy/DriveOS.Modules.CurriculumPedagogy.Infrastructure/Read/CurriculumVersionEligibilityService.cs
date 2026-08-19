using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.Modules.CurriculumPedagogy.Domain.Curricula;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class CurriculumVersionEligibilityService(CurriculumPedagogyDbContext db) : ICurriculumVersionEligibilityService
{
    public async Task<CurriculumVersionEligibility?> GetPublishedAsync(
        OrganizationId organizationId,
        CurriculumVersionId versionId,
        DateOnly pathStartDate,
        CancellationToken ct = default)
    {
        var row = await db.Curricula
            .AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .SelectMany(c => c.Versions.Where(v => v.Id == versionId && v.Status == CurriculumVersionStatus.Published),
                (c, v) => new
                {
                    Version = v,
                    CurriculumId = c.Id,
                    CurriculumCode = c.Code,
                    CurriculumName = c.Name
                })
            .SingleOrDefaultAsync(ct);

        if (row is null || pathStartDate < row.Version.EffectiveFrom ||
            (row.Version.EffectiveTo.HasValue && pathStartDate > row.Version.EffectiveTo.Value))
        {
            return null;
        }

        return new CurriculumVersionEligibility(
            row.Version.Id,
            row.CurriculumId,
            row.CurriculumCode,
            row.CurriculumName,
            row.Version.VersionNumber,
            row.Version.CountryCodeSnapshot,
            row.Version.LicenseCategoryCodeSnapshot,
            row.Version.EffectiveFrom,
            row.Version.EffectiveTo);
    }
}
