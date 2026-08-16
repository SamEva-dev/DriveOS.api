using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Students.GetStudentOverview;

public sealed class GetStudentOverviewQueryHandler(IStudentOverviewReadService readService)
    : IQueryHandler<GetStudentOverviewQuery, StudentOverviewResponse>
{
    public async Task<Result<StudentOverviewResponse>> Handle(
        GetStudentOverviewQuery query,
        CancellationToken cancellationToken
    )
    {
        StudentOverviewResponse? response = await readService.GetAsync(query, cancellationToken);
        return response is null
            ? Result.Failure<StudentOverviewResponse>(StudentOverviewErrors.NotFound)
            : Result.Success(response);
    }
}

public static class StudentOverviewErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Students.Overview.NotFound",
        "errors.students.overview.notFound"
    );
}
