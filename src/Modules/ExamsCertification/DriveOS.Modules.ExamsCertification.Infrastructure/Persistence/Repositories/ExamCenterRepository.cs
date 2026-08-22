using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamCenterRepository(ExamsCertificationDbContext dbContext) : IExamCenterRepository
{
    public Task<ExamCenter?> GetByIdAsync(OrganizationId organizationId, ExamCenterId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamCenters.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ExamCenter>> ListAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        await dbContext.ExamCenters.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<ExamCenter?> FindByExternalIdAsync(OrganizationId organizationId, string providerCode, string externalCenterId, CancellationToken cancellationToken = default) =>
        dbContext.ExamCenters.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExternalProviderCode == providerCode && x.ExternalCenterId == externalCenterId, cancellationToken);

    public Task<ExamCenter?> FindByExternalIdForUpdateAsync(OrganizationId organizationId, string providerCode, string externalCenterId, CancellationToken cancellationToken = default) =>
        dbContext.ExamCenters.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ExternalProviderCode == providerCode && x.ExternalCenterId == externalCenterId, cancellationToken);

    public void Add(ExamCenter center) => dbContext.ExamCenters.Add(center);
}
