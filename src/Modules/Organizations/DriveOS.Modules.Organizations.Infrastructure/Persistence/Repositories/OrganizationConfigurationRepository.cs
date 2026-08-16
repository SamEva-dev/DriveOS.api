using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationConfigurationRepository(OrganizationsDbContext dbContext)
    : IOrganizationConfigurationRepository
{
    public Task<OrganizationConfiguration?> GetForUpdateAsync(
        OrganizationConfigurationId configurationId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    ) =>
        dbContext.OrganizationConfigurations.SingleOrDefaultAsync(
            configuration =>
                configuration.Id == configurationId
                && configuration.OrganizationId == organizationId,
            cancellationToken
        );

    public Task<bool> VersionExistsAsync(
        OrganizationId organizationId,
        int versionNumber,
        CancellationToken cancellationToken = default
    ) =>
        dbContext
            .OrganizationConfigurations.AsNoTracking()
            .AnyAsync(
                configuration =>
                    configuration.OrganizationId == organizationId
                    && configuration.VersionNumber == versionNumber,
                cancellationToken
            );

    public async Task AddAsync(
        OrganizationConfiguration configuration,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await dbContext.OrganizationConfigurations.AddAsync(configuration, cancellationToken);
    }
}
