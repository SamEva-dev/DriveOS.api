using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Analytics;

public sealed class GetExamAnalyticsQueryHandler(IExamAnalyticsReadService readService)
    : IQueryHandler<GetExamAnalyticsQuery, ExamAnalyticsResponse>
{
    public async Task<Result<ExamAnalyticsResponse>> Handle(GetExamAnalyticsQuery query, CancellationToken cancellationToken)
        => Result.Success(await readService.GetAsync(query.OrganizationId, query.Filter, cancellationToken));
}
