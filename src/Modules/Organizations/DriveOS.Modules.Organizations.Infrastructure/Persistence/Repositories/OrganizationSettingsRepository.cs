using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationSettingsRepository(OrganizationsDbContext dbContext)
    : IOrganizationSettingsRepository
{
    public Task<OrganizationSettings?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext.OrganizationSettings.SingleOrDefaultAsync(
            settings => settings.OrganizationId == organizationId,
            cancellationToken
        );
    }

    public Task<bool> ExistsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .OrganizationSettings.AsNoTracking()
            .AnyAsync(settings => settings.OrganizationId == organizationId, cancellationToken);
    }

    public async Task AddAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(settings);

        await dbContext.OrganizationSettings.AddAsync(settings, cancellationToken);
    }
}
