using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Offers.TrackOffer;

public sealed record RecordCommercialOfferViewCommand(string SecureTokenHash) : ICommand;

public sealed record RecordCommercialOfferExchangeCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId,
    OfferInteractionType Type,
    string Summary,
    string? MetadataJson
) : ICommand;

public sealed record ScheduleCommercialOfferFollowUpCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId,
    DateTimeOffset NextFollowUpAtUtc,
    string? Note
) : ICommand;

public sealed record WithdrawCommercialOfferCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId,
    string Reason
) : ICommand;

public sealed record MarkCommercialOfferAcceptedCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId
) : ICommand;

public sealed record MarkCommercialOfferRejectedCommand(
    OrganizationId OrganizationId,
    CommercialOfferId OfferId,
    string Reason
) : ICommand;
