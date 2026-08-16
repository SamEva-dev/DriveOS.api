using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Statuses;

public sealed class GetStudentStatusesQueryHandler(IStudentStatusService s)
    : IQueryHandler<GetStudentStatusesQuery, StudentStatusesResponse>
{
    public async Task<Result<StudentStatusesResponse>> Handle(
        GetStudentStatusesQuery q,
        CancellationToken ct
    )
    {
        var v = await s.GetAsync(q.OrganizationId, q.StudentId, ct);
        return v is null
            ? Result.Failure<StudentStatusesResponse>(
                StudentStatusApplicationErrors.StudentNotFound
            )
            : Result.Success(v);
    }
}

public sealed class ApplyStudentBlockCommandHandler(IStudentStatusService s)
    : ICommandHandler<ApplyStudentBlockCommand, Guid>
{
    public Task<Result<Guid>> Handle(ApplyStudentBlockCommand c, CancellationToken ct) =>
        s.ApplyBlockAsync(c, ct);
}

public sealed class ReleaseStudentBlockCommandHandler(IStudentStatusService s)
    : ICommandHandler<ReleaseStudentBlockCommand>
{
    public Task<Result> Handle(ReleaseStudentBlockCommand c, CancellationToken ct) =>
        s.ReleaseBlockAsync(c, ct);
}

public sealed class OverrideStudentBlockCommandHandler(IStudentStatusService s)
    : ICommandHandler<OverrideStudentBlockCommand>
{
    public Task<Result> Handle(OverrideStudentBlockCommand c, CancellationToken ct) =>
        s.OverrideBlockAsync(c, ct);
}
