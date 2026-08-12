using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Offers.GetOffers;

internal sealed class GetCommercialOfferQueryHandler(ICommercialOfferRepository offers)
    : IQueryHandler<GetCommercialOfferQuery, CommercialOfferResponse>
{
    public async Task<Result<CommercialOfferResponse>> Handle(GetCommercialOfferQuery query, CancellationToken cancellationToken)
    {
        CommercialOffer? offer = await offers.GetByIdAsync(query.OrganizationId, query.OfferId, cancellationToken);
        return offer is null ? Result.Failure<CommercialOfferResponse>(CommercialOfferErrors.NotFound) : Result.Success(Map(offer));
    }

    internal static CommercialOfferResponse Map(CommercialOffer offer) => new(
        offer.Id.Value, offer.LeadId.Value, offer.AssessmentSessionId.Value,
        offer.AssessmentRevision, offer.BranchId?.Value, offer.Version,
        offer.TrainingCode, offer.CatalogAmount, offer.DiscountAmount,
        offer.TaxAmount, offer.Amount, offer.EstimatedFundingAmount,
        offer.ProspectRemainingAmount, offer.Currency, offer.ValidUntilUtc,
        offer.FinancingNotes, offer.Conditions, offer.Status.ToString(),
        offer.Lines.Select(x => new CommercialOfferLineResponse(x.Id.Value, x.Type.ToString(),
            x.ServiceId?.Value, x.Description, x.Quantity, x.Unit, x.UnitPrice,
            x.DiscountAmount, x.TaxRate, x.NetAmount, x.TaxAmount, x.TotalAmount,
            x.Mandatory, x.PriceSource.ToString(), x.ManualOverrideReason)).ToArray(),
        offer.DeliveryStatus?.ToString(), offer.DeliveryChannel?.ToString(), offer.DeliveryLanguage,
        offer.SentAtUtc, offer.SecureLinkExpiresAtUtc, offer.SecureLinkRevokedAtUtc,
        offer.DeliveryAttemptCount, offer.ViewedAtUtc, offer.LastViewedAtUtc,
        offer.ViewCount, offer.LastContactAtUtc, offer.NextFollowUpAtUtc,
        offer.Interactions.OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new OfferInteractionResponse(x.Id.Value, x.Type.ToString(),
                x.OccurredAtUtc, x.ActorUserId?.Value, x.Summary, x.MetadataJson)).ToArray());
}

internal sealed class GetLeadCommercialOffersQueryHandler(ICommercialOfferRepository offers)
    : IQueryHandler<GetLeadCommercialOffersQuery, IReadOnlyList<CommercialOfferResponse>>
{
    public async Task<Result<IReadOnlyList<CommercialOfferResponse>>> Handle(GetLeadCommercialOffersQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<CommercialOffer> result = await offers.GetByLeadAsync(query.OrganizationId, query.LeadId, cancellationToken);
        return Result.Success<IReadOnlyList<CommercialOfferResponse>>(result.Select(GetCommercialOfferQueryHandler.Map).ToArray());
    }
}
