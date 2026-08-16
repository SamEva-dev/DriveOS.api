using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Reactivations;

public sealed class GetEnrollmentReactivationsQueryHandler(IEnrollmentReactivationService s)
    : IQueryHandler<GetEnrollmentReactivationsQuery, IReadOnlyList<EnrollmentReactivationResponse>>
{
    public async Task<Result<IReadOnlyList<EnrollmentReactivationResponse>>> Handle(
        GetEnrollmentReactivationsQuery q,
        CancellationToken ct
    ) => Result.Success(await s.GetAsync(q, ct));
}

public sealed class CreateEnrollmentReactivationCommandHandler(IEnrollmentReactivationService s)
    : ICommandHandler<CreateEnrollmentReactivationCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateEnrollmentReactivationCommand c, CancellationToken ct) =>
        s.CreateAsync(c, ct);
}

public sealed class ReviewEnrollmentReactivationCheckCommandHandler(
    IEnrollmentReactivationService s
) : ICommandHandler<ReviewEnrollmentReactivationCheckCommand>
{
    public Task<Result> Handle(ReviewEnrollmentReactivationCheckCommand c, CancellationToken ct) =>
        s.ReviewCheckAsync(c, ct);
}

public sealed class ApplyEnrollmentReactivationCommandHandler(IEnrollmentReactivationService s)
    : ICommandHandler<ApplyEnrollmentReactivationCommand>
{
    public Task<Result> Handle(ApplyEnrollmentReactivationCommand c, CancellationToken ct) =>
        s.ApplyAsync(c, ct);
}
