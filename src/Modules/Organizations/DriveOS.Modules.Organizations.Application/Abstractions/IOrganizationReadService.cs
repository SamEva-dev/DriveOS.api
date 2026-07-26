using DriveOS.Modules.Organizations.Application
    .Organizations.GetOrganizationById;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Abstractions;

public interface IOrganizationReadService
{
    Task<OrganizationResponse?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}