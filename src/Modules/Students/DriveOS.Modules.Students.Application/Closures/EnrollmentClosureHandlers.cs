using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Closures;

public sealed class GetEnrollmentClosuresQueryHandler(IEnrollmentClosureService s)
    : IQueryHandler<GetEnrollmentClosuresQuery, IReadOnlyList<EnrollmentClosureResponse>>
{
    public async Task<Result<IReadOnlyList<EnrollmentClosureResponse>>> Handle(
        GetEnrollmentClosuresQuery q,
        CancellationToken ct
    ) => Result.Success(await s.GetAsync(q, ct));
}

public sealed class CreateEnrollmentClosureCommandHandler(IEnrollmentClosureService s)
    : ICommandHandler<CreateEnrollmentClosureCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateEnrollmentClosureCommand c, CancellationToken ct) =>
        s.CreateAsync(c, ct);
}

public sealed class ReviewEnrollmentClosureCheckCommandHandler(IEnrollmentClosureService s)
    : ICommandHandler<ReviewEnrollmentClosureCheckCommand>
{
    public Task<Result> Handle(ReviewEnrollmentClosureCheckCommand c, CancellationToken ct) =>
        s.ReviewCheckAsync(c, ct);
}

public sealed class CloseEnrollmentCommandHandler(IEnrollmentClosureService s)
    : ICommandHandler<CloseEnrollmentCommand>
{
    public Task<Result> Handle(CloseEnrollmentCommand c, CancellationToken ct) =>
        s.CloseAsync(c, ct);
}

public sealed class ArchiveStudentCommandHandler(IEnrollmentClosureService s)
    : ICommandHandler<ArchiveStudentCommand>
{
    public Task<Result> Handle(ArchiveStudentCommand c, CancellationToken ct) =>
        s.ArchiveAsync(c, ct);
}

public sealed class ReopenEnrollmentCommandHandler(IEnrollmentClosureService s)
    : ICommandHandler<ReopenEnrollmentCommand>
{
    public Task<Result> Handle(ReopenEnrollmentCommand c, CancellationToken ct) =>
        s.ReopenAsync(c, ct);
}
