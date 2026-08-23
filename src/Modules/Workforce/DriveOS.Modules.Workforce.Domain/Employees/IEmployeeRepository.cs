using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Domain.Employees;
public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(OrganizationId employerOrganizationId, EmployeeId employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdForUpdateAsync(OrganizationId employerOrganizationId, EmployeeId employeeId, CancellationToken cancellationToken = default);
    Task<Employee?> FindByEmployeeNumberAsync(OrganizationId employerOrganizationId, string employeeNumber, CancellationToken cancellationToken = default);
    Task<Employee?> FindLatestByPersonAsync(OrganizationId employerOrganizationId, PersonId personId, CancellationToken cancellationToken = default);
    Task<Employee?> FindCurrentByPersonAsync(OrganizationId employerOrganizationId, PersonId personId, CancellationToken cancellationToken = default);
    Task<Employee?> FindCurrentByUserAsync(OrganizationId employerOrganizationId, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> ListByPersonAsync(OrganizationId employerOrganizationId, PersonId personId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> ListAsync(OrganizationId employerOrganizationId, EmploymentStatus? status, CancellationToken cancellationToken = default);
    void Add(Employee employee);
}
