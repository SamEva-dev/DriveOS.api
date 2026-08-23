using DriveOS.Modules.Workforce.Domain.Offboarding;using DriveOS.SharedKernel.Identifiers;using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class OffboardingProcessRepository(WorkforceDbContext db):IOffboardingProcessRepository
{
 public Task<OffboardingProcess?> GetAsync(OrganizationId org,OffboardingProcessId id,bool tracking,CancellationToken ct=default){IQueryable<OffboardingProcess> q=db.OffboardingProcesses.Include(x=>x.Items);if(!tracking)q=q.AsNoTracking();return q.SingleOrDefaultAsync(x=>x.OrganizationId==org&&x.Id==id,ct);}
 public Task<OffboardingProcess?> FindCurrentByEmployeeAsync(OrganizationId org,EmployeeId employeeId,bool tracking,CancellationToken ct=default){IQueryable<OffboardingProcess> q=db.OffboardingProcesses.Include(x=>x.Items);if(!tracking)q=q.AsNoTracking();return q.Where(x=>x.OrganizationId==org&&x.EmployeeId==employeeId&&x.Status!=OffboardingStatus.Cancelled).OrderByDescending(x=>x.CreatedAtUtc).FirstOrDefaultAsync(ct);}
 public void Add(OffboardingProcess x)=>db.OffboardingProcesses.Add(x);
}
