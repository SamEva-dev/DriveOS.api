using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class NetworkOrganizationMembershipRepository(OrganizationsDbContext dbContext)
    : INetworkOrganizationMembershipRepository
{
    public Task<bool> HasActiveMembershipAsync(OrganizationId memberOrganizationId,
        CancellationToken cancellationToken = default) =>
        dbContext.NetworkOrganizationMemberships.AsNoTracking().AnyAsync(
            x => x.MemberOrganizationId == memberOrganizationId && x.EndedAtUtc == null,
            cancellationToken);

    public Task<NetworkOrganizationMembership?> GetActiveAsync(
        OrganizationId networkOrganizationId, OrganizationId memberOrganizationId,
        CancellationToken cancellationToken = default) =>
        dbContext.NetworkOrganizationMemberships.SingleOrDefaultAsync(
            x => x.NetworkOrganizationId == networkOrganizationId
                && x.MemberOrganizationId == memberOrganizationId
                && x.EndedAtUtc == null, cancellationToken);

    public async Task AddAsync(NetworkOrganizationMembership membership,
        CancellationToken cancellationToken = default) =>
        await dbContext.NetworkOrganizationMemberships.AddAsync(membership, cancellationToken);
}
