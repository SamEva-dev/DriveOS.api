using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles;

public interface IOrganizationLegalProfileReadService
{
    Task<OrganizationLegalProfileResponse?> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}
