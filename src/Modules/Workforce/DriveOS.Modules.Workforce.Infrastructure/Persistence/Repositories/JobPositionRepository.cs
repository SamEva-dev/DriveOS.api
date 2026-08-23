using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class JobPositionRepository(WorkforceDbContext db) : IJobPositionRepository
{
    public Task<JobPosition?> GetByIdAsync(OrganizationId org, JobPositionId id, CancellationToken ct = default)
        => db.JobPositions.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
    public Task<JobPosition?> GetByIdForUpdateAsync(OrganizationId org, JobPositionId id, CancellationToken ct = default)
        => db.JobPositions.SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
    public Task<JobPosition?> FindByCodeAsync(OrganizationId org, string code, CancellationToken ct = default)
    {
        string normalized = code.Trim().ToUpperInvariant();
        return db.JobPositions.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == org && x.Code == normalized, ct);
    }
    public async Task<IReadOnlyList<JobPosition>> ListAsync(OrganizationId org, JobPositionStatus? status, CancellationToken ct = default)
    {
        IQueryable<JobPosition> q = db.JobPositions.AsNoTracking().Where(x => x.OrganizationId == org);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        return await q.OrderBy(x => x.Name).ToListAsync(ct);
    }
    public void Add(JobPosition position) => db.JobPositions.Add(position);
}
