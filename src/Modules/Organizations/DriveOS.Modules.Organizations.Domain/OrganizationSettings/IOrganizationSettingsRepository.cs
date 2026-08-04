using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public interface IOrganizationSettingsRepository
{
    Task<OrganizationSettings?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationSettings settings,
        CancellationToken cancellationToken = default);
}
