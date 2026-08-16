using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Leads.QualifyLead;

public sealed class QualifyLeadCommandHandler(ILeadRepository repository, ICrmUnitOfWork unitOfWork)
    : ICommandHandler<QualifyLeadCommand>
{
    public async Task<Result> Handle(
        QualifyLeadCommand command,
        CancellationToken cancellationToken
    )
    {
        Lead? lead = await repository.GetByIdForUpdateAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );
        if (lead is null)
            return Result.Failure(LeadErrors.NotFound);

        Result<LeadQualification> qualification = LeadQualification.Create(
            command.Need,
            command.LicenseCategory,
            command.Availability,
            command.TargetDate,
            command.Financing,
            command.Notes
        );
        if (qualification.IsFailure)
            return Result.Failure(qualification.Error);

        Result result = lead.Qualify(qualification.Value);
        if (result.IsFailure)
            return result;

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
