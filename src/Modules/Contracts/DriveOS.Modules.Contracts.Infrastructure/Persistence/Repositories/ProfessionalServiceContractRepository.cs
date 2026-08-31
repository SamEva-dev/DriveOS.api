using DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalServiceContractRepository(
    ContractsDbContext db):IProfessionalServiceContractRepository
{
    public Task<ProfessionalServiceContract?> GetAsync(
        ProfessionalServiceContractId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.ProfessionalServiceContracts.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.ProfessionalServiceContracts.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<ProfessionalServiceContract?> GetByEngagementAsync(
        ProfessionalEngagementId engagementId,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.ProfessionalServiceContracts.SingleOrDefaultAsync(x=>x.EngagementId==engagementId,ct)
            :db.ProfessionalServiceContracts.AsNoTracking().SingleOrDefaultAsync(x=>x.EngagementId==engagementId,ct);

    public Task<bool> ExistsForEngagementAsync(
        ProfessionalEngagementId engagementId,CancellationToken ct=default)=>
        db.ProfessionalServiceContracts.AsNoTracking().AnyAsync(x=>x.EngagementId==engagementId,ct);

    public void Add(ProfessionalServiceContract contract)=>db.ProfessionalServiceContracts.Add(contract);
}
