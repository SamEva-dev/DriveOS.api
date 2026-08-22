using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;

internal sealed class ExamPlaceRepository(ExamsCertificationDbContext dbContext) : IExamPlaceRepository
{
    public Task<ExamPlace?> GetByIdAsync(OrganizationId organizationId, ExamPlaceId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaces.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamPlace?> GetByIdForUpdateAsync(OrganizationId organizationId, ExamPlaceId id, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaces.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<ExamPlace?> FindByExternalIdAsync(OrganizationId organizationId, string providerCode, string externalPlaceId, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaces.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ProviderCode == providerCode && x.ExternalPlaceId == externalPlaceId, cancellationToken);

    public Task<ExamPlace?> FindByExternalIdForUpdateAsync(OrganizationId organizationId, string providerCode, string externalPlaceId, CancellationToken cancellationToken = default) =>
        dbContext.ExamPlaces.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.ProviderCode == providerCode && x.ExternalPlaceId == externalPlaceId, cancellationToken);

    public async Task<IReadOnlyList<ExamPlace>> ListAvailableAsync(OrganizationId organizationId, DateTimeOffset fromUtc, DateTimeOffset toUtc, string? licenseCategory, CancellationToken cancellationToken = default)
    {
        IQueryable<ExamPlace> query = dbContext.ExamPlaces.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.StartsAtUtc >= fromUtc && x.StartsAtUtc < toUtc && (x.Status == ExamPlaceStatus.Available || x.Status == ExamPlaceStatus.Held));
        if (!string.IsNullOrWhiteSpace(licenseCategory)) query = query.Where(x => x.LicenseCategory == licenseCategory);
        return await query.OrderBy(x => x.StartsAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExamPlace>> ListExternalForUpdateAsync(OrganizationId organizationId, string providerCode,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, string? examCategory, IReadOnlyCollection<string>? centerExternalIds,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ExamPlace> query = dbContext.ExamPlaces.Where(x => x.OrganizationId == organizationId
            && x.ProviderCode == providerCode
            && x.StartsAtUtc >= fromUtc
            && x.StartsAtUtc < toUtc);

        if (!string.IsNullOrWhiteSpace(examCategory))
            query = query.Where(x => x.LicenseCategory == examCategory);

        if (centerExternalIds is { Count: > 0 })
        {
            string[] requested = centerExternalIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            IQueryable<ExamCenterId> centerIds = dbContext.ExamCenters.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId
                    && x.ExternalProviderCode == providerCode
                    && x.ExternalCenterId != null
                    && requested.Contains(x.ExternalCenterId))
                .Select(x => x.Id);
            query = query.Where(x => centerIds.Contains(x.ExamCenterId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public void Add(ExamPlace place) => dbContext.ExamPlaces.Add(place);
}
