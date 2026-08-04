using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings;

public interface IOrganizationSettingsReadService
{
    Task<OrganizationSettingsResponse?> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}
