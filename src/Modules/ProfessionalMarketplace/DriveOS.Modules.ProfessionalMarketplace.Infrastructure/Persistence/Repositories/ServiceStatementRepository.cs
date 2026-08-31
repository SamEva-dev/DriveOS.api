using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ServiceStatementRepository(ProfessionalMarketplaceDbContext db):IServiceStatementRepository
{
    public Task<ServiceStatement?> GetAsync(ServiceStatementId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ServiceStatements.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ServiceStatements.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> ExistsForPeriodAsync(ProfessionalEngagementId engagementId,DateOnly start,DateOnly end,CancellationToken ct=default)=>
        db.ServiceStatements.AsNoTracking().AnyAsync(x=>x.EngagementId==engagementId&&x.PeriodStart==start&&x.PeriodEnd==end,ct);
    public async Task<IReadOnlyList<ServiceStatement>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        await db.ServiceStatements.AsNoTracking().Where(x=>x.EngagementId==engagementId).OrderByDescending(x=>x.PeriodEnd).ToListAsync(ct);
    public async Task<IReadOnlyList<ServiceStatement>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default)=>
        await db.ServiceStatements.AsNoTracking().Where(x=>x.ProfessionalProfileId==profileId).OrderByDescending(x=>x.PeriodEnd).ThenByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public void Add(ServiceStatement statement)=>db.ServiceStatements.Add(statement);
}
