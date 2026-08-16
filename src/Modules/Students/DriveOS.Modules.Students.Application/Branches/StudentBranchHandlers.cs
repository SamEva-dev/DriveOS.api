using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Branches;

public sealed class GetStudentBranchesQueryHandler(IStudentBranchService s)
    : IQueryHandler<GetStudentBranchesQuery, StudentBranchesResponse>
{
    public async Task<Result<StudentBranchesResponse>> Handle(
        GetStudentBranchesQuery q,
        CancellationToken ct
    )
    {
        var x = await s.GetAsync(q, ct);
        return x is null
            ? Result.Failure<StudentBranchesResponse>(
                StudentBranchApplicationErrors.StudentNotFound
            )
            : Result.Success(x);
    }
}

public sealed class AssignStudentBranchCommandHandler(IStudentBranchService s)
    : ICommandHandler<AssignStudentBranchCommand, Guid>
{
    public Task<Result<Guid>> Handle(AssignStudentBranchCommand c, CancellationToken ct) =>
        s.AssignAsync(c, ct);
}

public sealed class AnalyzePrimaryBranchChangeCommandHandler(IStudentBranchService s)
    : ICommandHandler<AnalyzePrimaryBranchChangeCommand, PrimaryBranchChangeAnalysisResponse>
{
    public Task<Result<PrimaryBranchChangeAnalysisResponse>> Handle(
        AnalyzePrimaryBranchChangeCommand c,
        CancellationToken ct
    ) => s.AnalyzePrimaryChangeAsync(c, ct);
}

public sealed class ChangePrimaryStudentBranchCommandHandler(IStudentBranchService s)
    : ICommandHandler<ChangePrimaryStudentBranchCommand>
{
    public Task<Result> Handle(ChangePrimaryStudentBranchCommand c, CancellationToken ct) =>
        s.ChangePrimaryAsync(c, ct);
}

public sealed class EndStudentBranchAssignmentCommandHandler(IStudentBranchService s)
    : ICommandHandler<EndStudentBranchAssignmentCommand>
{
    public Task<Result> Handle(EndStudentBranchAssignmentCommand c, CancellationToken ct) =>
        s.EndAsync(c, ct);
}
