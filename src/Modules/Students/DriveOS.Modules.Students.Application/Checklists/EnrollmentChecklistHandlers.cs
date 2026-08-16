using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Checklists;

public sealed class GetEnrollmentChecklistQueryHandler(IEnrollmentChecklistService s)
    : IQueryHandler<GetEnrollmentChecklistQuery, EnrollmentChecklistResponse>
{
    public async Task<Result<EnrollmentChecklistResponse>> Handle(
        GetEnrollmentChecklistQuery q,
        CancellationToken ct
    )
    {
        var v = await s.GetAsync(q, ct);
        return v is null
            ? Result.Failure<EnrollmentChecklistResponse>(
                EnrollmentChecklistApplicationErrors.EnrollmentNotFound
            )
            : Result.Success(v);
    }
}

public sealed class SynchronizeEnrollmentChecklistCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<SynchronizeEnrollmentChecklistCommand, int>
{
    public Task<Result<int>> Handle(
        SynchronizeEnrollmentChecklistCommand c,
        CancellationToken ct
    ) => s.SynchronizeAsync(c, ct);
}

public sealed class ChangeChecklistItemStatusCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<ChangeChecklistItemStatusCommand>
{
    public Task<Result> Handle(ChangeChecklistItemStatusCommand c, CancellationToken ct) =>
        s.ChangeStatusAsync(c, ct);
}

public sealed class AssignChecklistItemCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<AssignChecklistItemCommand>
{
    public Task<Result> Handle(AssignChecklistItemCommand c, CancellationToken ct) =>
        s.AssignAsync(c, ct);
}

public sealed class RemindChecklistItemCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<RemindChecklistItemCommand>
{
    public Task<Result> Handle(RemindChecklistItemCommand c, CancellationToken ct) =>
        s.RemindAsync(c, ct);
}

public sealed class ActivateEnrollmentCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<ActivateEnrollmentCommand>
{
    public Task<Result> Handle(ActivateEnrollmentCommand c, CancellationToken ct) =>
        s.ActivateAsync(c, ct);
}

public sealed class ConfigureChecklistRuleCommandHandler(IEnrollmentChecklistService s)
    : ICommandHandler<ConfigureChecklistRuleCommand, Guid>
{
    public Task<Result<Guid>> Handle(ConfigureChecklistRuleCommand c, CancellationToken ct) =>
        s.ConfigureRuleAsync(c, ct);
}
