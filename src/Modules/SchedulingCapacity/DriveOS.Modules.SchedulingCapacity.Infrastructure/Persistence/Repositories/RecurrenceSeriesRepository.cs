using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Repositories;
internal sealed class RecurrenceSeriesRepository(SchedulingCapacityDbContext db) : IRecurrenceSeriesRepository
{
    public Task<RecurrenceSeries?> GetByIdAsync(RecurrenceSeriesId id, OrganizationId org, CancellationToken ct=default)=>db.RecurrenceSeries.AsNoTracking().Include(x=>x.Occurrences).Include(x=>x.Resources).SingleOrDefaultAsync(x=>x.Id==id&&x.OrganizationId==org,ct);
    public Task<RecurrenceSeries?> GetByIdForUpdateAsync(RecurrenceSeriesId id, OrganizationId org, CancellationToken ct=default)=>db.RecurrenceSeries.Include(x=>x.Occurrences).Include(x=>x.Resources).SingleOrDefaultAsync(x=>x.Id==id&&x.OrganizationId==org,ct);
    public void Add(RecurrenceSeries series)=>db.RecurrenceSeries.Add(series);
}
