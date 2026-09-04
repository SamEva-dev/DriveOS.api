using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Persistence;

namespace DriveOS.Modules.Organizations.Domain.Organizations;

public interface IOrganizationRepository : IRepository<Organization, OrganizationId>
{
    Task<bool> ExistsByLegalNameAsync(
        string legalName,
        string countryCode,
        CancellationToken cancellationToken = default
    );

    Task<Organization?> GetByProvisioningKeyAsync(
        string idempotencyKey,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<Organization?>(null);
}
