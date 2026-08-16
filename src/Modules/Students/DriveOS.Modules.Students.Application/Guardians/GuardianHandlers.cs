using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Guardians;

public sealed class GetGuardiansQueryHandler(IGuardianService service)
    : IQueryHandler<GetGuardiansQuery, GuardianListResponse>
{
    public async Task<Result<GuardianListResponse>> Handle(
        GetGuardiansQuery q,
        CancellationToken ct
    )
    {
        var value = await service.GetAsync(q.OrganizationId, q.StudentId, ct);
        return value is null
            ? Result.Failure<GuardianListResponse>(GuardianApplicationErrors.StudentNotFound)
            : Result.Success(value);
    }
}

public sealed class CreateGuardianCommandHandler(IGuardianService service)
    : ICommandHandler<CreateGuardianCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateGuardianCommand c, CancellationToken ct) =>
        service.CreateAsync(c, ct);
}

public sealed class UpdateGuardianCommandHandler(IGuardianService service)
    : ICommandHandler<UpdateGuardianCommand>
{
    public Task<Result> Handle(UpdateGuardianCommand c, CancellationToken ct) =>
        service.UpdateAsync(c, ct);
}

public sealed class RevokeGuardianCommandHandler(IGuardianService service)
    : ICommandHandler<RevokeGuardianCommand>
{
    public Task<Result> Handle(RevokeGuardianCommand c, CancellationToken ct) =>
        service.RevokeAsync(c, ct);
}

public sealed class InviteGuardianCommandHandler(IGuardianService service)
    : ICommandHandler<InviteGuardianCommand>
{
    public Task<Result> Handle(InviteGuardianCommand c, CancellationToken ct) =>
        service.InviteAsync(c, ct);
}
