using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;

public sealed class GetCrmDashboardQueryHandler(ICrmDashboardReadService readService, IClock clock)
    : IQueryHandler<GetCrmDashboardQuery, CrmDashboardResponse>
{
    public async Task<Result<CrmDashboardResponse>> Handle(
        GetCrmDashboardQuery query,
        CancellationToken cancellationToken
    )
    {
        CrmDashboardResponse response = await readService.GetAsync(
            query.OrganizationIds,
            string.Equals(query.Scope, "branch", StringComparison.OrdinalIgnoreCase)
                ? query.BranchId
                : null,
            query.Filters,
            clock.UtcNow,
            cancellationToken
        );

        return Result.Success(response);
    }
}
