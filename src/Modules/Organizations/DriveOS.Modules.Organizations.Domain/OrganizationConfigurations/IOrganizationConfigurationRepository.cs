using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;

public interface IOrganizationConfigurationRepository
{
    Task<OrganizationConfiguration?> GetForUpdateAsync(
        OrganizationConfigurationId configurationId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> VersionExistsAsync(
        OrganizationId organizationId,
        int versionNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationConfiguration configuration,
        CancellationToken cancellationToken = default);
}
