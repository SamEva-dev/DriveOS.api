using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.CreateVariant;

internal sealed class CreateCommercialOfferVariantCommandHandler(
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateCommercialOfferVariantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommercialOfferVariantCommand command, CancellationToken cancellationToken)
    {
        CommercialOffer? source = await offers.GetByIdAsync(command.OrganizationId, command.SourceOfferId, cancellationToken);
        if (source is null) return Result.Failure<Guid>(CommercialOfferErrors.NotFound);
        if (source.Status is not (CommercialOfferStatus.Draft or CommercialOfferStatus.InternalReview or
            CommercialOfferStatus.Approved or CommercialOfferStatus.Sent or CommercialOfferStatus.Viewed or
            CommercialOfferStatus.Negotiation))
            return Result.Failure<Guid>(CommercialOfferErrors.InvalidTransition);

        string[] mandatoryDescriptions = source.Lines.Where(x => x.Mandatory)
            .Select(x => x.Description.Trim()).ToArray();
        bool removesMandatoryLine = mandatoryDescriptions.Any(required =>
            !command.Lines.Any(x => x.Mandatory && string.Equals(x.Description.Trim(), required, StringComparison.OrdinalIgnoreCase)));
        if (removesMandatoryLine) return Result.Failure<Guid>(CommercialOfferErrors.MandatoryLineRequired);

        int version = await offers.GetNextVersionAsync(command.OrganizationId, source.LeadId, cancellationToken);
        Result<CommercialOffer> variant = CommercialOffer.Generate(
            CommercialOfferId.New(), command.OrganizationId, source.LeadId,
            source.AssessmentSessionId, source.AssessmentRevision, source.BranchId,
            version, command.TrainingCode, source.Currency, command.ValidUntilUtc,
            clock.UtcNow, command.EstimatedFundingAmount, command.FinancingNotes,
            command.Conditions, command.InternalNotes, command.Lines);
        if (variant.IsFailure) return Result.Failure<Guid>(variant.Error);

        offers.Add(variant.Value);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(variant.Value.Id.Value);
    }
}
