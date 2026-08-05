using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives;

public interface IOrganizationRepresentativeReadService
{
    Task<OrganizationRepresentativeResponse?> GetByIdAsync(
        OrganizationId organizationId,
        OrganizationRepresentativeId representativeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrganizationRepresentativeListItem>> GetListAsync(
        OrganizationId organizationId,
        OrganizationRepresentativeStatus? status,
        CancellationToken cancellationToken = default);
}
