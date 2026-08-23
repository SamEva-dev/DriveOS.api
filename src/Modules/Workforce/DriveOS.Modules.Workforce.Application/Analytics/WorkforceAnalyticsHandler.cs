using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Application.Analytics;

public sealed class GetWorkforceAnalyticsQueryHandler(IWorkforceAnalyticsReadService readService)
    : IQueryHandler<GetWorkforceAnalyticsQuery, WorkforceAnalyticsResponse>
{
    public async Task<Result<WorkforceAnalyticsResponse>> Handle(
        GetWorkforceAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.To < query.From)
            return Result.Failure<WorkforceAnalyticsResponse>(WorkforceAnalyticsErrors.InvalidPeriod);

        if (query.To.DayNumber - query.From.DayNumber + 1 > 730)
            return Result.Failure<WorkforceAnalyticsResponse>(WorkforceAnalyticsErrors.PeriodTooLarge);

        return Result.Success(await readService.GetAsync(
            query.OrganizationId,
            query.From,
            query.To,
            cancellationToken));
    }
}
