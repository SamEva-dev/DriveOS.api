using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalApplicationRepository(ProfessionalMarketplaceDbContext db):IProfessionalApplicationRepository
{
    public Task<ProfessionalApplication?> GetAsync(ProfessionalApplicationId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalApplications.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalApplications.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> ExistsAsync(ProfessionalOpportunityId opportunityId,ProfessionalProfileId profileId,CancellationToken ct=default)=>db.ProfessionalApplications.AnyAsync(x=>x.OpportunityId==opportunityId&&x.ProfessionalProfileId==profileId,ct);
    public async Task<IReadOnlyList<ProfessionalApplication>> ListByOpportunityAsync(ProfessionalOpportunityId opportunityId,CancellationToken ct=default)=>await db.ProfessionalApplications.AsNoTracking().Where(x=>x.OpportunityId==opportunityId).OrderBy(x=>x.SubmittedAtUtc).ToListAsync(ct);
    public void Add(ProfessionalApplication x)=>db.ProfessionalApplications.Add(x);
}
