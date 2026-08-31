using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ExternalAccessGrantRepository(ProfessionalMarketplaceDbContext db):IExternalAccessGrantRepository
{
    public Task<ExternalAccessGrant?> GetAsync(ExternalAccessGrantId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ExternalAccessGrants.SingleOrDefaultAsync(x=>x.Id==id,ct):
        db.ExternalAccessGrants.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> ExistsActiveAsync(ProfessionalEngagementId engagementId,ExternalAccessResourceType resourceType,Guid resourceId,string permission,CancellationToken ct=default)
    {
        permission=(permission??string.Empty).Trim().ToUpperInvariant();
        return db.ExternalAccessGrants.AnyAsync(x=>x.EngagementId==engagementId&&x.ResourceType==resourceType&&x.ResourceId==resourceId&&x.Permission==permission&&x.Status==ExternalAccessGrantStatus.Active,ct);
    }

    public async Task<IReadOnlyList<ExternalAccessGrant>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        await db.ExternalAccessGrants.AsNoTracking().Where(x=>x.EngagementId==engagementId).OrderBy(x=>x.ResourceType).ThenBy(x=>x.CreatedAtUtc).ToListAsync(ct);

    public Task<bool> HasEffectiveGrantAsync(ProfessionalProfileId profileId,OrganizationId organizationId,ExternalAccessResourceType resourceType,Guid resourceId,string permission,DateOnly date,CancellationToken ct=default)
    {
        permission=(permission??string.Empty).Trim().ToUpperInvariant();
        return db.ExternalAccessGrants.AsNoTracking().AnyAsync(x=>
            x.ProfessionalProfileId==profileId&&x.OrganizationId==organizationId&&
            x.ResourceType==resourceType&&x.ResourceId==resourceId&&x.Permission==permission&&
            x.Status==ExternalAccessGrantStatus.Active&&x.StartDate<=date&&date<=x.EndDate,ct);
    }

    public void Add(ExternalAccessGrant grant)=>db.ExternalAccessGrants.Add(grant);
}
