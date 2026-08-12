using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.CreateActivity;

public sealed class CreateCrmActivityCommandHandler(
    ILeadRepository leads,
    ICrmActivityRepository activities,
    ICrmUnitOfWork unitOfWork)
    : ICommandHandler<CreateCrmActivityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCrmActivityCommand command, CancellationToken ct)
    {
        if (await leads.GetByIdAsync(command.OrganizationId, command.LeadId, ct) is null)
            return Result.Failure<Guid>(LeadErrors.NotFound);

        Result<CrmActivity> result = CrmActivity.Create(CrmActivityId.New(), command.OrganizationId,
            command.LeadId, command.Type, command.Direction, command.Subject,
            command.Details, command.OccurredAtUtc);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        activities.Add(result.Value);
        await unitOfWork.CommitAsync(ct);
        return Result.Success(result.Value.Id.Value);
    }
}
