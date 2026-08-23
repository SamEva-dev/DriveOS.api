using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class LeaveRequestRepository(WorkforceDbContext db) : ILeaveRequestRepository
{
    public Task<LeaveRequest?> GetByIdAsync(OrganizationId org, LeaveRequestId id, CancellationToken ct = default)
        => db.LeaveRequests.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
    public Task<LeaveRequest?> GetByIdForUpdateAsync(OrganizationId org, LeaveRequestId id, CancellationToken ct = default)
        => db.LeaveRequests.SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
    public async Task<IReadOnlyList<LeaveRequest>> ListAsync(OrganizationId org, EmployeeId? employeeId, LeaveRequestStatus? status, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        IQueryable<LeaveRequest> q = db.LeaveRequests.AsNoTracking().Where(x => x.OrganizationId == org);
        if (employeeId is { } e) q = q.Where(x => x.EmployeeId == e);
        if (status is { } s) q = q.Where(x => x.Status == s);
        if (from is { } f) q = q.Where(x => x.EndDate >= f);
        if (to is { } t) q = q.Where(x => x.StartDate <= t);
        return await q.OrderByDescending(x => x.StartDate).ThenBy(x => x.EmployeeId).ToListAsync(ct);
    }
    public Task<bool> HasOverlappingAsync(OrganizationId org, EmployeeId employeeId, DateOnly startDate, DateOnly endDate, LeaveRequestId excludeId, CancellationToken ct = default)
        => db.LeaveRequests.AsNoTracking().AnyAsync(x => x.OrganizationId == org && x.EmployeeId == employeeId && x.Id != excludeId && x.Status != LeaveRequestStatus.Cancelled && x.Status != LeaveRequestStatus.Rejected && x.Status != LeaveRequestStatus.Draft && x.StartDate <= endDate && x.EndDate >= startDate, ct);
    public void Add(LeaveRequest request) => db.LeaveRequests.Add(request);
}
