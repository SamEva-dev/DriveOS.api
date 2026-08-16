using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.CreateLead;

public sealed class CreateLeadCommandHandler(
    ILeadRepository leadRepository,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<CreateLeadCommand, LeadId>
{
    public async Task<Result<LeadId>> Handle(
        CreateLeadCommand command,
        CancellationToken cancellationToken
    )
    {
        Result<LeadIdentity> identityResult = LeadIdentity.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        if (identityResult.IsFailure)
        {
            return Result.Failure<LeadId>(identityResult.Error);
        }

        Result<RequestedTraining> trainingResult = RequestedTraining.Create(
            command.LicenseCategory,
            command.Transmission,
            command.PreferredLocation
        );

        if (trainingResult.IsFailure)
        {
            return Result.Failure<LeadId>(trainingResult.Error);
        }

        Result<LeadSource> sourceResult = LeadSource.Create(
            command.SourceType,
            command.SourceDetail
        );

        if (sourceResult.IsFailure)
        {
            return Result.Failure<LeadId>(sourceResult.Error);
        }

        Result<Lead> leadResult = Lead.Create(
            LeadId.New(),
            command.OrganizationId,
            command.BranchId,
            identityResult.Value,
            trainingResult.Value,
            sourceResult.Value,
            command.AssignedAdvisorId
        );

        if (leadResult.IsFailure)
        {
            return Result.Failure<LeadId>(leadResult.Error);
        }

        await leadRepository.AddAsync(leadResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(leadResult.Value.Id);
    }
}
