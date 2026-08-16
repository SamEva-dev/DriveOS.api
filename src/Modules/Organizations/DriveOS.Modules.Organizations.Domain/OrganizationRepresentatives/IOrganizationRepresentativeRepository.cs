using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

public interface IOrganizationRepresentativeRepository
{
    Task<OrganizationRepresentative?> GetForUpdateAsync(
        OrganizationRepresentativeId representativeId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsActiveAsync(
        OrganizationId organizationId,
        PersonId personId,
        OrganizationRepresentativeType representativeType,
        CancellationToken cancellationToken = default
    );

    Task<int> CountActiveOwnersAsync(
        OrganizationId organizationId,
        OrganizationRepresentativeId? excludingRepresentativeId = null,
        CancellationToken cancellationToken = default
    );

    Task<OrganizationRepresentative?> GetPrimaryOwnerForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        OrganizationRepresentative representative,
        CancellationToken cancellationToken = default
    );
}
