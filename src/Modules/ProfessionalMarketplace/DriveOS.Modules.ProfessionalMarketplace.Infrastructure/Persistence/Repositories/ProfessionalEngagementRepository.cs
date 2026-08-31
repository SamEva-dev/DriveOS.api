using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalEngagementRepository(ProfessionalMarketplaceDbContext db):IProfessionalEngagementRepository
{
    public Task<ProfessionalEngagement?> GetAsync(ProfessionalEngagementId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ProfessionalEngagements.SingleOrDefaultAsync(x=>x.Id==id,ct):
        db.ProfessionalEngagements.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> ExistsForCommercialOfferAsync(ProfessionalCommercialOfferId offerId,CancellationToken ct=default)=>
        db.ProfessionalEngagements.AnyAsync(x=>x.CommercialOfferId==offerId,ct);

    public async Task<IReadOnlyList<ProfessionalEngagement>> ListByOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default)=>
        await db.ProfessionalEngagements.AsNoTracking()
            .Where(x=>x.OrganizationId==organizationId)
            .OrderByDescending(x=>x.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProfessionalEngagement>> ListByProfileAsync(
        ProfessionalProfileId profileId,
        CancellationToken ct=default)=>
        await db.ProfessionalEngagements.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId)
            .OrderByDescending(x=>x.CreatedAtUtc)
            .ToListAsync(ct);

    public void Add(ProfessionalEngagement x)=>db.ProfessionalEngagements.Add(x);
}
