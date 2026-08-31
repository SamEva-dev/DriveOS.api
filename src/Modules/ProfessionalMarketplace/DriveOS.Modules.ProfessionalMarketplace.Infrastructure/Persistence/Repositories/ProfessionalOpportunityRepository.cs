using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalOpportunityRepository(ProfessionalMarketplaceDbContext db):IProfessionalOpportunityRepository
{
    public Task<ProfessionalOpportunity?> GetAsync(ProfessionalOpportunityId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ProfessionalOpportunities.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalOpportunities.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public async Task<IReadOnlyList<ProfessionalOpportunity>> ListPublishedAsync(string? countryCode,string? categoryCode,CancellationToken ct=default)
    {
        var q=db.ProfessionalOpportunities.AsNoTracking().Where(x=>x.Status==ProfessionalOpportunityStatus.Published);
        if(!string.IsNullOrWhiteSpace(countryCode)){var c=countryCode.Trim().ToUpperInvariant();q=q.Where(x=>x.CountryCode==c);}
        if(!string.IsNullOrWhiteSpace(categoryCode)){var c=categoryCode.Trim().ToUpperInvariant();q=q.Where(x=>x.TeachingCategoryCodes.Contains(c));}
        return await q.OrderBy(x=>x.StartsOn).ThenBy(x=>x.Title).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProfessionalOpportunity>> ListForOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default) =>
        await db.ProfessionalOpportunities.AsNoTracking()
            .Where(x=>x.OrganizationId==organizationId)
            .OrderByDescending(x=>x.CreatedAtUtc)
            .ThenBy(x=>x.Title)
            .ToListAsync(ct);

    public void Add(ProfessionalOpportunity x)=>db.ProfessionalOpportunities.Add(x);
}
