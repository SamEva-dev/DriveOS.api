using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Students.GetStudents;

public sealed class GetStudentsQueryHandler(IStudentReadService readService)
    : IQueryHandler<GetStudentsQuery, PagedResult<StudentListItem>>
{
    public async Task<Result<PagedResult<StudentListItem>>> Handle(
        GetStudentsQuery query,
        CancellationToken cancellationToken
    ) => Result.Success(await readService.GetPageAsync(query, cancellationToken));
}
