using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Workforce.Domain.LeaveRequests;
public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(OrganizationId organizationId, LeaveRequestId id, CancellationToken ct = default);
    Task<LeaveRequest?> GetByIdForUpdateAsync(OrganizationId organizationId, LeaveRequestId id, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> ListAsync(OrganizationId organizationId, EmployeeId? employeeId, LeaveRequestStatus? status, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<bool> HasOverlappingAsync(OrganizationId organizationId, EmployeeId employeeId, DateOnly startDate, DateOnly endDate, LeaveRequestId excludeId, CancellationToken ct = default);
    void Add(LeaveRequest request);
}
