using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives;

internal sealed class OrganizationRepresentativeReadService(OrganizationsDbContext dbContext)
    : IOrganizationRepresentativeReadService
{
    public Task<OrganizationRepresentativeResponse?> GetByIdAsync(OrganizationId organizationId,OrganizationRepresentativeId representativeId,CancellationToken cancellationToken=default)=>
        dbContext.OrganizationRepresentatives.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.Id==representativeId)
        .Select(x=>new OrganizationRepresentativeResponse(x.Id,x.OrganizationId,x.PersonId,x.UserId,x.RepresentativeType,x.AuthorityScope.Value,x.IsPrimaryOwner,x.EffectiveFrom,x.EffectiveTo,x.Status,x.Revision,x.CreatedAtUtc,x.CreatedByUserId,x.LastModifiedAtUtc,x.LastModifiedByUserId))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<OrganizationRepresentativeListItem>> GetListAsync(OrganizationId organizationId,OrganizationRepresentativeStatus? status,CancellationToken cancellationToken=default)
    {
        IQueryable<OrganizationRepresentative> query=dbContext.OrganizationRepresentatives.AsNoTracking().Where(x=>x.OrganizationId==organizationId);
        if(status.HasValue)query=query.Where(x=>x.Status==status.Value);
        return await query.OrderByDescending(x=>x.IsPrimaryOwner).ThenBy(x=>x.RepresentativeType).ThenBy(x=>x.EffectiveFrom)
            .Select(x=>new OrganizationRepresentativeListItem(x.Id,x.PersonId,x.UserId,x.RepresentativeType,x.AuthorityScope.Value,x.IsPrimaryOwner,x.EffectiveFrom,x.EffectiveTo,x.Status,x.Revision)).ToListAsync(cancellationToken);
    }
}
