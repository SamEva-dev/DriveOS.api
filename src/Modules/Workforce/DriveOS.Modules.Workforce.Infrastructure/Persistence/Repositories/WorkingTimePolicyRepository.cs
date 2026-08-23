using DriveOS.Modules.Workforce.Domain.WorkingTime; using DriveOS.Modules.Workforce.Infrastructure.Persistence; using DriveOS.SharedKernel.Identifiers; using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class WorkingTimePolicyRepository(WorkforceDbContext db):IWorkingTimePolicyRepository
{
 public Task<WorkingTimePolicy?> GetAsync(OrganizationId o,WorkingTimePolicyId id,bool tracking,CancellationToken ct=default)=> (tracking?db.WorkingTimePolicies:db.WorkingTimePolicies.AsNoTracking()).SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.Id==id,ct);
 public async Task<IReadOnlyList<WorkingTimePolicy>> ListAsync(OrganizationId o,EmployeeId e,CancellationToken ct=default)=>await db.WorkingTimePolicies.AsNoTracking().Where(x=>x.OrganizationId==o&&x.EmployeeId==e).OrderByDescending(x=>x.EffectiveFrom).ToListAsync(ct);
 public Task<bool> HasOverlapAsync(OrganizationId o,EmployeeId e,DateOnly from,DateOnly? to,WorkingTimePolicyId? excluding,CancellationToken ct=default){var end=to??DateOnly.MaxValue;return db.WorkingTimePolicies.AsNoTracking().AnyAsync(x=>x.OrganizationId==o&&x.EmployeeId==e&&x.Status==WorkingTimePolicyStatus.Active&&(!excluding.HasValue||x.Id!=excluding.Value)&&x.EffectiveFrom<=end&&(!x.EffectiveTo.HasValue||x.EffectiveTo.Value>=from),ct);}
 public void Add(WorkingTimePolicy p)=>db.WorkingTimePolicies.Add(p);
}
