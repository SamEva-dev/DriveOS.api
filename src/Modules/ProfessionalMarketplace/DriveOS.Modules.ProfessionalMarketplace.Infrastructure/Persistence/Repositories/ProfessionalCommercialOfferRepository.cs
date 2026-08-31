using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalCommercialOfferRepository(ProfessionalMarketplaceDbContext db):IProfessionalCommercialOfferRepository
{
    public Task<ProfessionalCommercialOffer?> GetAsync(ProfessionalCommercialOfferId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalCommercialOffers.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalCommercialOffers.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> ActiveOfferExistsForApplicationAsync(ProfessionalApplicationId id,CancellationToken ct=default)=>db.ProfessionalCommercialOffers.AnyAsync(x=>x.ApplicationId==id&&x.Status!=ProfessionalCommercialOfferStatus.Cancelled&&x.Status!=ProfessionalCommercialOfferStatus.Finalized,ct);
    public Task<bool> ActiveOfferExistsForProposalAsync(ProfessionalProposalId id,CancellationToken ct=default)=>db.ProfessionalCommercialOffers.AnyAsync(x=>x.ProposalId==id&&x.Status!=ProfessionalCommercialOfferStatus.Cancelled&&x.Status!=ProfessionalCommercialOfferStatus.Finalized,ct);
    public async Task<IReadOnlyList<ProfessionalCommercialOffer>> ListAsync(
        OrganizationId organizationId,
        ProfessionalProfileId professionalProfileId,
        ProfessionalApplicationId? applicationId,
        ProfessionalProposalId? proposalId,
        ProfessionalOpportunityId? opportunityId,
        CancellationToken ct=default)
    {
        IQueryable<ProfessionalCommercialOffer> query=db.ProfessionalCommercialOffers.AsNoTracking()
            .Where(x=>x.OrganizationId==organizationId&&x.ProfessionalProfileId==professionalProfileId);
        if(applicationId is not null)query=query.Where(x=>x.ApplicationId==applicationId);
        if(proposalId is not null)query=query.Where(x=>x.ProposalId==proposalId);
        if(opportunityId is not null)query=query.Where(x=>x.OpportunityId==opportunityId);
        return await query.OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    }
    public void Add(ProfessionalCommercialOffer x)=>db.ProfessionalCommercialOffers.Add(x);
}
