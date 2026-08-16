using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.GetOffers;

public sealed record GetCommercialOfferQuery(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId
) : IQuery<CommercialOfferResponse>;

public sealed record GetLeadCommercialOffersQuery(OrganizationId OrganizationId, LeadId LeadId)
    : IQuery<IReadOnlyList<CommercialOfferResponse>>;

public sealed record CommercialOfferResponse(
    Guid Id,
    Guid LeadId,
    Guid AssessmentSessionId,
    int AssessmentRevision,
    Guid? BranchId,
    int Version,
    string TrainingCode,
    decimal CatalogAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Amount,
    decimal EstimatedFundingAmount,
    decimal ProspectRemainingAmount,
    string Currency,
    DateTimeOffset ValidUntilUtc,
    string? FinancingNotes,
    string? Conditions,
    string Status,
    IReadOnlyCollection<CommercialOfferLineResponse> Lines,
    string? DeliveryStatus,
    string? DeliveryChannel,
    string? DeliveryLanguage,
    DateTimeOffset? SentAtUtc,
    DateTimeOffset? SecureLinkExpiresAtUtc,
    DateTimeOffset? SecureLinkRevokedAtUtc,
    int DeliveryAttemptCount,
    DateTimeOffset? ViewedAtUtc,
    DateTimeOffset? LastViewedAtUtc,
    int ViewCount,
    DateTimeOffset? LastContactAtUtc,
    DateTimeOffset? NextFollowUpAtUtc,
    IReadOnlyCollection<OfferInteractionResponse> Interactions
);

public sealed record CommercialOfferLineResponse(
    Guid Id,
    string Type,
    Guid? ServiceId,
    string Description,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    bool Mandatory,
    string PriceSource,
    string? ManualOverrideReason
);

public sealed record OfferInteractionResponse(
    Guid Id,
    string Type,
    DateTimeOffset OccurredAtUtc,
    Guid? ActorUserId,
    string? Summary,
    string? MetadataJson
);
