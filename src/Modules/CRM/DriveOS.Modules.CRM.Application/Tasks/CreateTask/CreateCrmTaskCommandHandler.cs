using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Tasks.CreateTask;

public sealed class CreateCrmTaskCommandHandler(ILeadRepository leads, ICrmTaskRepository tasks, ICrmUnitOfWork unitOfWork)
    : ICommandHandler<CreateCrmTaskCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCrmTaskCommand command, CancellationToken ct)
    {
        if (await leads.GetByIdAsync(command.OrganizationId, command.LeadId, ct) is null)
            return Result.Failure<Guid>(LeadErrors.NotFound);
        Result<CrmTask> result = CrmTask.Create(CrmTaskId.New(), command.OrganizationId, command.LeadId,
            command.Type, command.Title, command.Notes, command.DueAtUtc, command.AssignedToUserId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        tasks.Add(result.Value); await unitOfWork.CommitAsync(ct); return Result.Success(result.Value.Id.Value);
    }
}
