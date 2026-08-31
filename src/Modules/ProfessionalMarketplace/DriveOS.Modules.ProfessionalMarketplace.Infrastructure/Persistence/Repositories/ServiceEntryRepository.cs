using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ServiceEntryRepository(ProfessionalMarketplaceDbContext db):IServiceEntryRepository
{
    public Task<ServiceEntry?> GetAsync(ServiceEntryId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ServiceEntries.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ServiceEntries.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> ExistsForSourceAsync(ProfessionalEngagementId engagementId,ServiceEntrySourceType sourceType,Guid sourceId,CancellationToken ct=default)=>
        db.ServiceEntries.AsNoTracking().AnyAsync(x=>x.EngagementId==engagementId&&x.SourceType==sourceType&&x.SourceId==sourceId,ct);
    public async Task<IReadOnlyList<ServiceEntry>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        await db.ServiceEntries.AsNoTracking().Where(x=>x.EngagementId==engagementId).OrderByDescending(x=>x.ServiceDate).ThenByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public async Task<IReadOnlyList<ServiceEntry>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default)=>
        await db.ServiceEntries.AsNoTracking().Where(x=>x.ProfessionalProfileId==profileId).OrderByDescending(x=>x.ServiceDate).ThenByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public async Task<IReadOnlyList<ServiceEntry>> ListByMissionAsync(ProfessionalMissionId missionId,CancellationToken ct=default)=>
        await db.ServiceEntries.AsNoTracking().Where(x=>x.MissionId==missionId).OrderByDescending(x=>x.ServiceDate).ThenByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public void Add(ServiceEntry entry)=>db.ServiceEntries.Add(entry);
}
