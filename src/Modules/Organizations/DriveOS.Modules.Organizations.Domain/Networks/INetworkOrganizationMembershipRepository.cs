using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Networks;

public interface INetworkOrganizationMembershipRepository
{
    Task<bool> HasActiveMembershipAsync(OrganizationId memberOrganizationId,
        CancellationToken cancellationToken = default);

    Task<NetworkOrganizationMembership?> GetActiveAsync(OrganizationId networkOrganizationId,
        OrganizationId memberOrganizationId, CancellationToken cancellationToken = default);

    Task AddAsync(NetworkOrganizationMembership membership,
        CancellationToken cancellationToken = default);
}
