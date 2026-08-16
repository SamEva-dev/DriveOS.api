using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Application.Abstractions.Persistence;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.ChangeStatus;

internal sealed class SubmitCommercialOfferForReviewCommandHandler(
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<SubmitCommercialOfferForReviewCommand>
{
    public async Task<Result> Handle(
        SubmitCommercialOfferForReviewCommand command,
        CancellationToken cancellationToken
    )
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(
            command.OrganizationId,
            command.OfferId,
            cancellationToken
        );
        if (offer is null)
            return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.SubmitForReview();
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class ApproveCommercialOfferCommandHandler(
    ICommercialOfferRepository offers,
    ICrmUnitOfWork unitOfWork
) : ICommandHandler<ApproveCommercialOfferCommand>
{
    public async Task<Result> Handle(
        ApproveCommercialOfferCommand command,
        CancellationToken cancellationToken
    )
    {
        CommercialOffer? offer = await offers.GetForUpdateAsync(
            command.OrganizationId,
            command.OfferId,
            cancellationToken
        );
        if (offer is null)
            return Result.Failure(CommercialOfferErrors.NotFound);
        Result result = offer.Approve();
        if (result.IsFailure)
            return result;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
