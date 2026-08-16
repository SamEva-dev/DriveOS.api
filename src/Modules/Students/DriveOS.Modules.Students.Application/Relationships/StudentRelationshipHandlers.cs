using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Relationships;

public sealed class GetStudentRelationshipsQueryHandler(IStudentRelationshipService s)
    : IQueryHandler<GetStudentRelationshipsQuery, StudentRelationshipListResponse>
{
    public async Task<Result<StudentRelationshipListResponse>> Handle(
        GetStudentRelationshipsQuery q,
        CancellationToken ct
    )
    {
        var v = await s.GetAsync(q.OrganizationId, q.StudentId, ct);
        return v is null
            ? Result.Failure<StudentRelationshipListResponse>(
                StudentRelationshipApplicationErrors.StudentNotFound
            )
            : Result.Success(v);
    }
}

public sealed class CreateStudentRelationshipCommandHandler(IStudentRelationshipService s)
    : ICommandHandler<CreateStudentRelationshipCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateStudentRelationshipCommand c, CancellationToken ct) =>
        s.CreateAsync(c, ct);
}

public sealed class UpdateStudentRelationshipCommandHandler(IStudentRelationshipService s)
    : ICommandHandler<UpdateStudentRelationshipCommand>
{
    public Task<Result> Handle(UpdateStudentRelationshipCommand c, CancellationToken ct) =>
        s.UpdateAsync(c, ct);
}

public sealed class SuspendStudentRelationshipCommandHandler(IStudentRelationshipService s)
    : ICommandHandler<SuspendStudentRelationshipCommand>
{
    public Task<Result> Handle(SuspendStudentRelationshipCommand c, CancellationToken ct) =>
        s.SuspendAsync(c, ct);
}

public sealed class RevokeStudentRelationshipCommandHandler(IStudentRelationshipService s)
    : ICommandHandler<RevokeStudentRelationshipCommand>
{
    public Task<Result> Handle(RevokeStudentRelationshipCommand c, CancellationToken ct) =>
        s.RevokeAsync(c, ct);
}

public sealed class InviteStudentRelationshipCommandHandler(IStudentRelationshipService s)
    : ICommandHandler<InviteStudentRelationshipCommand>
{
    public Task<Result> Handle(InviteStudentRelationshipCommand c, CancellationToken ct) =>
        s.InviteAsync(c, ct);
}
