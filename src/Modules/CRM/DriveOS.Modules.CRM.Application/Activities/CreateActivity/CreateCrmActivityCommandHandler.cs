using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.CreateActivity;

public sealed class CreateCrmActivityCommandHandler(
    ILeadRepository leads,
    ICrmActivityRepository activities,
    ICrmTaskRepository tasks,
    ICrmUnitOfWork unitOfWork,
    IClock clock
) : ICommandHandler<CreateCrmActivityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCrmActivityCommand command, CancellationToken ct)
    {
        if (command.OccurredAtUtc > clock.UtcNow.AddMinutes(1))
            return Result.Failure<Guid>(CrmActivityErrors.OccurredAtInFuture);

        if (
            command.LeadId.HasValue
            && await leads.GetByIdAsync(command.OrganizationId, command.LeadId.Value, ct) is null
        )
            return Result.Failure<Guid>(LeadErrors.NotFound);

        Result<CrmActivity> result = CrmActivity.Create(
            CrmActivityId.New(),
            command.OrganizationId,
            command.LeadId,
            command.Type,
            command.Direction,
            command.Subject,
            command.Details,
            command.OccurredAtUtc,
            command.AdvisorUserId,
            command.Metadata
        );

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        activities.Add(result.Value);
        if (
            !string.IsNullOrWhiteSpace(command.NextActionTitle)
            && command.NextActionDueAtUtc.HasValue
        )
        {
            if (!command.LeadId.HasValue)
                return Result.Failure<Guid>(CrmActivityErrors.NextActionRequiresLead);
            Result<CrmTask> task = CrmTask.Create(
                CrmTaskId.New(),
                command.OrganizationId,
                command.LeadId.Value,
                command.NextActionType,
                command.NextActionTitle,
                command.Details,
                command.NextActionDueAtUtc.Value,
                command.AdvisorUserId
            );
            if (task.IsFailure)
                return Result.Failure<Guid>(task.Error);
            tasks.Add(task.Value);
        }
        await unitOfWork.CommitAsync(ct);
        return Result.Success(result.Value.Id.Value);
    }
}
