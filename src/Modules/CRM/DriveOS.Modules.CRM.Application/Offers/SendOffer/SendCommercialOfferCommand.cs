using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.SendOffer;

public sealed record SendCommercialOfferCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId,
    OfferDeliveryChannel Channel,
    IReadOnlyCollection<OfferRecipientDraft> Recipients,
    string Subject,
    string Message,
    string Language,
    string DocumentReference,
    IReadOnlyCollection<string> AttachmentReferences,
    int SecureLinkLifetimeHours
) : ICommand<SendCommercialOfferResponse>;

public sealed record SendCommercialOfferResponse(
    Guid OfferId,
    string OfferStatus,
    string DeliveryStatus,
    string? SecureLinkToken,
    DateTimeOffset? SecureLinkExpiresAtUtc
);
