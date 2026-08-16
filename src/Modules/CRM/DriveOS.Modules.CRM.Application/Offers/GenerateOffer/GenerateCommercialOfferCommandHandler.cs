using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.GenerateOffer;

internal sealed class GenerateCommercialOfferCommandHandler(
    IAssessmentSessionRepository sessions,
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<GenerateCommercialOfferCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        GenerateCommercialOfferCommand command,
        CancellationToken cancellationToken
    )
    {
        AssessmentSession? session = await sessions.GetByIdAsync(
            command.OrganizationId,
            command.AssessmentSessionId,
            cancellationToken
        );
        if (session is null)
            return Result.Failure<Guid>(AssessmentSessionErrors.NotFound);
        if (session.LeadId != command.LeadId)
            return Result.Failure<Guid>(CommercialOfferErrors.LeadMismatch);
        if (
            session.ResultStatus
            is not (AssessmentResultStatus.Validated or AssessmentResultStatus.Shared)
        )
            return Result.Failure<Guid>(CommercialOfferErrors.AssessmentResultMustBeValidated);

        int version = await offers.GetNextVersionAsync(
            command.OrganizationId,
            command.LeadId,
            cancellationToken
        );
        Result<CommercialOffer> generated = CommercialOffer.Generate(
            CommercialOfferId.New(),
            command.OrganizationId,
            command.LeadId,
            session.Id,
            session.Revision,
            command.BranchId,
            version,
            command.TrainingCode,
            command.Currency,
            command.ValidUntilUtc,
            DateTimeOffset.UtcNow,
            command.EstimatedFundingAmount,
            command.FinancingNotes,
            command.Conditions,
            command.InternalNotes,
            command.Lines
        );
        if (generated.IsFailure)
            return Result.Failure<Guid>(generated.Error);

        offers.Add(generated.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(generated.Value.Id.Value);
    }
}
