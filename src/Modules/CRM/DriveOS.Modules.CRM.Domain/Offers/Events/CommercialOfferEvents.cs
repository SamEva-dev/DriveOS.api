using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Offers.Events;

public sealed record CommercialOfferCreatedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    int Version,
    decimal Amount,
    string Currency
) : DomainEvent;

public sealed record CommercialOfferSentDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset SentAtUtc
) : DomainEvent;

public sealed record CommercialOfferDeliveryPreparedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    OfferDeliveryChannel Channel,
    int RecipientCount,
    string Language,
    DateTimeOffset? LinkExpiresAtUtc
) : DomainEvent;

public sealed record CommercialOfferSecureLinkRevokedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset RevokedAtUtc
) : DomainEvent;

public sealed record CommercialOfferAcceptedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset AcceptedAtUtc
) : DomainEvent;

public sealed record CommercialOfferRejectedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset RejectedAtUtc
) : DomainEvent;

public sealed record CommercialOfferViewedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    int ViewCount,
    DateTimeOffset ViewedAtUtc
) : DomainEvent;

public sealed record CommercialOfferModificationRequestedDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    OfferInteractionId InteractionId
) : DomainEvent;

public sealed record CommercialOfferWithdrawnDomainEvent(
    CommercialOfferId OfferId,
    OrganizationId OrganizationId,
    LeadId LeadId,
    DateTimeOffset WithdrawnAtUtc
) : DomainEvent;
