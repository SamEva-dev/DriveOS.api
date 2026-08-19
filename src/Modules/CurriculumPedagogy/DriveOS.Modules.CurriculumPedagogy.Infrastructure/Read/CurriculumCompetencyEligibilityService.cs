using DriveOS.Modules.CurriculumPedagogy.Application.Competencies;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class CurriculumCompetencyEligibilityService(Persistence.CurriculumPedagogyDbContext db) : ICurriculumCompetencyEligibilityService
{
    public async Task<CurriculumCompetencyEligibility?> GetAsync(OrganizationId organizationId, CurriculumVersionId curriculumVersionId, CompetencyId competencyId, CancellationToken cancellationToken = default)
    {
        var curricula = await db.Curricula.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .Include(x => x.Versions).ThenInclude(x => x.Modules).ThenInclude(x => x.Competencies)
            .ToListAsync(cancellationToken);

        var competency = curricula.SelectMany(x => x.Versions)
            .Where(x => x.Id == curriculumVersionId)
            .SelectMany(x => x.Modules).SelectMany(x => x.Competencies)
            .SingleOrDefault(x => x.Id == competencyId);

        return competency is null ? null : new CurriculumCompetencyEligibility(curriculumVersionId, competency.Id, competency.Code, competency.Name, competency.IsRequired);
    }
}
