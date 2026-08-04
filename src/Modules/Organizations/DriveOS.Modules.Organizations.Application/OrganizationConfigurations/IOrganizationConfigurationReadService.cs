using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations;

public interface IOrganizationConfigurationReadService
{
    Task<OrganizationConfigurationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationConfigurationId configurationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationConfigurationListItemResponse>> GetVersionsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}
