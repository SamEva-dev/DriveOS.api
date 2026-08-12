using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;

public interface ICrmDashboardReadService
{
    Task<CrmDashboardResponse> GetAsync(IReadOnlyCollection<OrganizationId> organizationIds,
        Guid? branchId, CrmDashboardFilters filters, DateTimeOffset nowUtc,
        CancellationToken cancellationToken);
}
