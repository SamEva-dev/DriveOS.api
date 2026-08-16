using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Students.Application.Dashboard.GetDashboard;

public interface IStudentDashboardReadService
{
    Task<StudentDashboardResponse> GetAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        CancellationToken cancellationToken = default
    );
}
