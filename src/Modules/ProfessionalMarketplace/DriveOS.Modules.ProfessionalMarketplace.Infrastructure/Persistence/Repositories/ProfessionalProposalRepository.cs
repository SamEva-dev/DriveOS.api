using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalProposalRepository(ProfessionalMarketplaceDbContext db):IProfessionalProposalRepository
{
    public Task<ProfessionalProposal?> GetAsync(ProfessionalProposalId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalProposals.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalProposals.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<ProfessionalProposal>> ListAsync(OrganizationId organizationId,ProfessionalProfileId profileId,ProfessionalOpportunityId? opportunityId,CancellationToken ct=default)=>
        await db.ProfessionalProposals.AsNoTracking()
            .Where(x=>x.OrganizationId==organizationId&&x.ProfessionalProfileId==profileId&&(opportunityId==null||x.OpportunityId==opportunityId))
            .OrderByDescending(x=>x.SentAtUtc).ToListAsync(ct);
    public Task<bool> OpenProposalExistsAsync(OrganizationId organizationId,ProfessionalProfileId profileId,ProfessionalOpportunityId? opportunityId,CancellationToken ct=default)=>
        db.ProfessionalProposals.AnyAsync(x=>x.OrganizationId==organizationId&&x.ProfessionalProfileId==profileId&&x.OpportunityId==opportunityId&&(x.Status==ProfessionalProposalStatus.Sent||x.Status==ProfessionalProposalStatus.Countered),ct);
    public void Add(ProfessionalProposal x)=>db.ProfessionalProposals.Add(x);
}
