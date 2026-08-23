using DriveOS.Modules.Workforce.Domain.Timesheets; using DriveOS.SharedKernel.Identifiers; using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class TimesheetRepository(WorkforceDbContext db):ITimesheetRepository
{
 public Task<Timesheet?> GetAsync(OrganizationId o,TimesheetId id,bool tracking,CancellationToken ct=default){IQueryable<Timesheet> q=db.Timesheets.Include(x=>x.Entries);if(!tracking)q=q.AsNoTracking();return q.SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.Id==id,ct);}
 public async Task<IReadOnlyList<Timesheet>> ListAsync(OrganizationId o,EmployeeId? employeeId,TimesheetStatus? status,DateOnly? from,DateOnly? to,CancellationToken ct=default){var q=db.Timesheets.AsNoTracking().Include(x=>x.Entries).Where(x=>x.OrganizationId==o);if(employeeId.HasValue)q=q.Where(x=>x.EmployeeId==employeeId.Value);if(status.HasValue)q=q.Where(x=>x.Status==status.Value);if(from.HasValue)q=q.Where(x=>x.PeriodTo>=from.Value);if(to.HasValue)q=q.Where(x=>x.PeriodFrom<=to.Value);return await q.OrderByDescending(x=>x.PeriodFrom).ToListAsync(ct);}
 public Task<bool> HasOverlapAsync(OrganizationId o,EmployeeId e,DateOnly from,DateOnly to,TimesheetId? excluding,CancellationToken ct=default)=>db.Timesheets.AsNoTracking().AnyAsync(x=>x.OrganizationId==o&&x.EmployeeId==e&&(!excluding.HasValue||x.Id!=excluding.Value)&&x.PeriodFrom<=to&&x.PeriodTo>=from,ct);
 public void Add(Timesheet x)=>db.Timesheets.Add(x);
}
