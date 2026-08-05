using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationClosureRepository(OrganizationsDbContext dbContext) : IOrganizationClosureRepository
{
    public Task<OrganizationClosure?> GetForUpdateAsync(OrganizationClosureId closureId, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationClosures.SingleOrDefaultAsync(x => x.Id == closureId, cancellationToken);

    public Task<OrganizationClosure?> GetByIdAsync(OrganizationClosureId closureId, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationClosures.AsNoTracking().SingleOrDefaultAsync(x => x.Id == closureId, cancellationToken);

    public async Task<IReadOnlyList<OrganizationClosure>> ListByOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        await dbContext.OrganizationClosures.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<OrganizationClosure?> GetOpenByOrganizationAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationClosures.SingleOrDefaultAsync(x => x.OrganizationId == organizationId &&
            (x.Status == OrganizationClosureStatus.Draft || x.Status == OrganizationClosureStatus.UnderReview ||
             x.Status == OrganizationClosureStatus.Approved || x.Status == OrganizationClosureStatus.Scheduled), cancellationToken);

    public Task<bool> HasOpenClosureAsync(OrganizationId organizationId, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationClosures.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId &&
            (x.Status == OrganizationClosureStatus.Draft || x.Status == OrganizationClosureStatus.UnderReview ||
             x.Status == OrganizationClosureStatus.Approved || x.Status == OrganizationClosureStatus.Scheduled), cancellationToken);

    public Task AddAsync(OrganizationClosure closure, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationClosures.AddAsync(closure, cancellationToken).AsTask();
}
