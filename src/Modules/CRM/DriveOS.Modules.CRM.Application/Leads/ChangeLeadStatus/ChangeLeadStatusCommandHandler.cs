using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.ChangeLeadStatus;

public sealed class ChangeLeadStatusCommandHandler(
    ILeadRepository leadRepository,
    ICrmUnitOfWork unitOfWork) : ICommandHandler<ChangeLeadStatusCommand>
{
    public async Task<Result> Handle(
        ChangeLeadStatusCommand command,
        CancellationToken cancellationToken)
    {
        Lead? lead = await leadRepository.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken);

        if (lead is null)
        {
            return Result.Failure(LeadErrors.NotFound);
        }

        Result result = lead.ChangeStatus(command.TargetStatus, command.Reason);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
