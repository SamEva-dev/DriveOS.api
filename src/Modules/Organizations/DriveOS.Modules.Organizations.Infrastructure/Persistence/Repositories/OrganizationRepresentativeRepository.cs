using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepresentativeRepository(OrganizationsDbContext dbContext)
    : IOrganizationRepresentativeRepository
{
    public Task<OrganizationRepresentative?> GetForUpdateAsync(
        OrganizationRepresentativeId representativeId,
        OrganizationId organizationId,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationRepresentatives.SingleOrDefaultAsync(
            x => x.Id == representativeId && x.OrganizationId == organizationId,
            cancellationToken);

    public Task<bool> ExistsActiveAsync(
        OrganizationId organizationId,
        PersonId personId,
        OrganizationRepresentativeType representativeType,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationRepresentatives.AsNoTracking().AnyAsync(
            x => x.OrganizationId == organizationId &&
                 x.PersonId == personId &&
                 x.RepresentativeType == representativeType &&
                 x.Status != OrganizationRepresentativeStatus.Ended,
            cancellationToken);

    public Task<int> CountActiveOwnersAsync(
        OrganizationId organizationId,
        OrganizationRepresentativeId? excludingRepresentativeId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationRepresentatives.AsNoTracking().CountAsync(
            x => x.OrganizationId == organizationId &&
                 x.RepresentativeType == OrganizationRepresentativeType.Owner &&
                 x.Status == OrganizationRepresentativeStatus.Active &&
                 (!excludingRepresentativeId.HasValue || x.Id != excludingRepresentativeId.Value),
            cancellationToken);

    public Task<OrganizationRepresentative?> GetPrimaryOwnerForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default) =>
        dbContext.OrganizationRepresentatives.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId &&
                 x.IsPrimaryOwner &&
                 x.Status == OrganizationRepresentativeStatus.Active,
            cancellationToken);

    public async Task AddAsync(OrganizationRepresentative representative, CancellationToken cancellationToken = default) =>
        await dbContext.OrganizationRepresentatives.AddAsync(representative, cancellationToken);
}
