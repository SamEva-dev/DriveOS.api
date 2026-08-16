using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Suspensions;

public sealed class GetEnrollmentSuspensionsQueryHandler(IEnrollmentSuspensionService service)
    : IQueryHandler<GetEnrollmentSuspensionsQuery, IReadOnlyList<EnrollmentSuspensionResponse>>
{
    public async Task<Result<IReadOnlyList<EnrollmentSuspensionResponse>>> Handle(
        GetEnrollmentSuspensionsQuery query,
        CancellationToken ct
    ) => Result.Success(await service.GetAsync(query, ct));
}

public sealed class SuspendEnrollmentCommandHandler(IEnrollmentSuspensionService service)
    : ICommandHandler<SuspendEnrollmentCommand, Guid>
{
    public Task<Result<Guid>> Handle(SuspendEnrollmentCommand command, CancellationToken ct) =>
        service.SuspendAsync(command, ct);
}
