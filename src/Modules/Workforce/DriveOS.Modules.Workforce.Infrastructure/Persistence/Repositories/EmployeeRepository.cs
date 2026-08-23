using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class EmployeeRepository(WorkforceDbContext db) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(OrganizationId org, EmployeeId id, CancellationToken ct = default)
        => db.Employees.AsNoTracking().Include(x => x.BranchAssignments).Include(x => x.JobPositionAssignments).Include(x => x.Qualifications).Include(x => x.InstructorAuthorizations).Include(x => x.EmploymentContracts).SingleOrDefaultAsync(x => x.EmployerOrganizationId == org && x.Id == id, ct);

    public Task<Employee?> GetByIdForUpdateAsync(OrganizationId org, EmployeeId id, CancellationToken ct = default)
        => db.Employees.Include(x => x.BranchAssignments).Include(x => x.JobPositionAssignments).Include(x => x.Qualifications).Include(x => x.InstructorAuthorizations).Include(x => x.EmploymentContracts).SingleOrDefaultAsync(x => x.EmployerOrganizationId == org && x.Id == id, ct);

    public Task<Employee?> FindByEmployeeNumberAsync(OrganizationId org, string number, CancellationToken ct = default)
    {
        string normalized = number.Trim().ToUpperInvariant();
        return db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.EmployerOrganizationId == org && x.EmployeeNumber == normalized && x.Status != EmploymentStatus.Ended, ct);
    }

    public Task<Employee?> FindLatestByPersonAsync(OrganizationId org, PersonId personId, CancellationToken ct = default)
        => db.Employees.AsNoTracking()
            .Where(x => x.EmployerOrganizationId == org && x.PersonId == personId)
            .OrderByDescending(x => x.EmploymentStartDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

    public Task<Employee?> FindCurrentByPersonAsync(OrganizationId org, PersonId personId, CancellationToken ct = default)
        => db.Employees.AsNoTracking().Where(x => x.EmployerOrganizationId == org && x.PersonId == personId && x.Status != EmploymentStatus.Ended).OrderByDescending(x => x.EmploymentStartDate).FirstOrDefaultAsync(ct);

    public Task<Employee?> FindCurrentByUserAsync(OrganizationId org, UserId userId, CancellationToken ct = default)
        => db.Employees.AsNoTracking().Where(x => x.EmployerOrganizationId == org && x.UserId == userId && x.Status != EmploymentStatus.Ended).OrderByDescending(x => x.EmploymentStartDate).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Employee>> ListByPersonAsync(OrganizationId org, PersonId personId, CancellationToken ct = default)
        => await db.Employees.AsNoTracking().Where(x => x.EmployerOrganizationId == org && x.PersonId == personId)
            .OrderByDescending(x => x.EmploymentStartDate).ThenByDescending(x => x.CreatedAtUtc).ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> ListAsync(OrganizationId org, EmploymentStatus? status, CancellationToken ct = default)
    {
        IQueryable<Employee> q = db.Employees.AsNoTracking().Where(x => x.EmployerOrganizationId == org);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        return await q.OrderBy(x => x.EmployeeNumber).ToListAsync(ct);
    }

    public void Add(Employee employee) => db.Employees.Add(employee);
}
