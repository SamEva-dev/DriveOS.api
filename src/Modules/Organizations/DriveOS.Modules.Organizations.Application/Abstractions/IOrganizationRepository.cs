using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Abstractions;

public interface IOrganizationRepository
{
    Task<bool> ExistsByLegalNameAsync(
        string legalName,
        string countryCode,
        CancellationToken cancellationToken = default);

    Task<Organization?> GetByIdAsync(
        OrganizationId organizationId,
        QueryTracking tracking = QueryTracking.NoTracking,
        CancellationToken cancellationToken = default);

    void Add(Organization organization);
}