using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Application.Dashboard;

public sealed class GetWorkforceDashboardQueryHandler(
    IWorkforceDashboardReadService readService,
    IClock clock)
    : IQueryHandler<GetWorkforceDashboardQuery, WorkforceDashboardResponse>
{
    public async Task<Result<WorkforceDashboardResponse>> Handle(
        GetWorkforceDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var days = Math.Clamp(query.AlertWindowDays, 1, 365);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        return Result.Success(await readService.GetAsync(
            query.OrganizationId,
            today,
            days,
            cancellationToken));
    }
}
