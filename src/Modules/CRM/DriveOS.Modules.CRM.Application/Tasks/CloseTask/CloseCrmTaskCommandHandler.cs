using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Tasks.CloseTask;

public sealed class CloseCrmTaskCommandHandler(
    ICrmTaskRepository repository,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<CloseCrmTaskCommand>
{
    public async Task<Result> Handle(CloseCrmTaskCommand command, CancellationToken ct)
    {
        CrmTask? task = await repository.GetByIdForUpdateAsync(
            command.OrganizationId,
            new CrmTaskId(command.TaskId.Value),
            ct
        );
        if (task is null)
            return Result.Failure(CrmTaskErrors.NotFound);
        Result result = command.Cancel
            ? task.Cancel(DateTimeOffset.UtcNow)
            : task.Complete(DateTimeOffset.UtcNow);
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(ct);
        return Result.Success();
    }
}
