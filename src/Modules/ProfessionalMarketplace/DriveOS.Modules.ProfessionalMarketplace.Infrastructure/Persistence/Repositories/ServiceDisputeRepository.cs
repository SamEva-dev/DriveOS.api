using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ServiceDisputeRepository(ProfessionalMarketplaceDbContext db):IServiceDisputeRepository
{
    public Task<ServiceDispute?> GetAsync(ServiceDisputeId id,bool tracking,CancellationToken ct=default)=>
        tracking?db.ServiceDisputes.SingleOrDefaultAsync(x=>x.Id==id,ct):
        db.ServiceDisputes.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> HasOpenDisputeAsync(ServiceEntryId serviceEntryId,CancellationToken ct=default)=>
        db.ServiceDisputes.AsNoTracking().AnyAsync(x=>x.ServiceEntryId==serviceEntryId&&
            x.Status!=ServiceDisputeStatus.Resolved&&x.Status!=ServiceDisputeStatus.Rejected,ct);

    public async Task<IReadOnlyList<ServiceDispute>> ListByOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default)=>
        await db.ServiceDisputes.AsNoTracking().Where(x=>x.ClientOrganizationId==organizationId)
            .OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);

    public async Task<IReadOnlyList<ServiceDispute>> ListByProfessionalAsync(ProfessionalProfileId profileId,CancellationToken ct=default)=>
        await db.ServiceDisputes.AsNoTracking().Where(x=>x.ProfessionalProfileId==profileId)
            .OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);

    public void Add(ServiceDispute dispute)=>db.ServiceDisputes.Add(dispute);
}
