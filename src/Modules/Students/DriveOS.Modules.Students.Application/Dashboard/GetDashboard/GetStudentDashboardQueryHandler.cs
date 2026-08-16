using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Dashboard.GetDashboard;

public sealed class GetStudentDashboardQueryHandler(IStudentDashboardReadService readService)
    : IQueryHandler<GetStudentDashboardQuery, StudentDashboardResponse>
{
    public async Task<Result<StudentDashboardResponse>> Handle(
        GetStudentDashboardQuery query,
        CancellationToken cancellationToken
    ) =>
        Result.Success(
            await readService.GetAsync(query.OrganizationId, query.BranchId, cancellationToken)
        );
}
