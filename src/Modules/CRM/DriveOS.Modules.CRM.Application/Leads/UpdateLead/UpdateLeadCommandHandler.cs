using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.UpdateLead;

public sealed class UpdateLeadCommandHandler(
    ILeadRepository leadRepository,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<UpdateLeadCommand>
{
    public async Task<Result> Handle(UpdateLeadCommand command, CancellationToken cancellationToken)
    {
        Lead? lead = await leadRepository.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );

        if (lead is null)
        {
            return Result.Failure(LeadErrors.NotFound);
        }

        Result<LeadIdentity> identityResult = LeadIdentity.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        if (identityResult.IsFailure)
        {
            return Result.Failure(identityResult.Error);
        }

        Result<RequestedTraining> trainingResult = RequestedTraining.Create(
            command.LicenseCategory,
            command.Transmission,
            command.PreferredLocation
        );

        if (trainingResult.IsFailure)
        {
            return Result.Failure(trainingResult.Error);
        }

        Result<LeadSource> sourceResult = LeadSource.Create(
            command.SourceType,
            command.SourceDetail
        );

        if (sourceResult.IsFailure)
        {
            return Result.Failure(sourceResult.Error);
        }

        Result updateResult = lead.UpdateInformation(
            command.BranchId,
            identityResult.Value,
            trainingResult.Value,
            sourceResult.Value
        );

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
