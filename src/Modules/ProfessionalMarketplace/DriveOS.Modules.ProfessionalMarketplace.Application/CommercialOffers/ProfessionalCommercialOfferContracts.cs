using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.CommercialOffers;

public sealed record CreateProfessionalCommercialOfferCommand(
    ProfessionalCommercialOfferId Id,
    OrganizationId OrganizationId,
    ProfessionalProfileId ProfessionalProfileId,
    ProfessionalApplicationId? ApplicationId,
    ProfessionalProposalId? ProposalId,
    ProfessionalOpportunityId? OpportunityId,
    CommercialOfferTerms Terms,
    UserId ActorUserId):ICommand<ProfessionalCommercialOfferId>;

public sealed record ReviseProfessionalCommercialOfferCommand(
    ProfessionalCommercialOfferId Id,OrganizationId OrganizationId,CommercialOfferTerms Terms,UserId ActorUserId):ICommand;
public sealed record SendProfessionalCommercialOfferCommand(ProfessionalCommercialOfferId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record AcceptCommercialOfferByOrganizationCommand(ProfessionalCommercialOfferId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record AcceptCommercialOfferByProfessionalCommand(ProfessionalCommercialOfferId Id,ProfessionalProfileId ProfileId,UserId ActorUserId):ICommand;
public sealed record FinalizeProfessionalCommercialOfferCommand(ProfessionalCommercialOfferId Id,OrganizationId OrganizationId,UserId ActorUserId):ICommand;
public sealed record CancelProfessionalCommercialOfferCommand(ProfessionalCommercialOfferId Id,OrganizationId OrganizationId,string Reason,UserId ActorUserId):ICommand;

public sealed record ListProfessionalCommercialOffersQuery(
    OrganizationId OrganizationId,
    ProfessionalProfileId ProfessionalProfileId,
    ProfessionalApplicationId? ApplicationId,
    ProfessionalProposalId? ProposalId,
    ProfessionalOpportunityId? OpportunityId):IQuery<IReadOnlyList<ProfessionalCommercialOfferResponse>>;

public sealed record ProfessionalCommercialOfferResponse(
    Guid Id,
    Guid OrganizationId,
    Guid ProfessionalProfileId,
    Guid? ApplicationId,
    Guid? ProposalId,
    Guid? OpportunityId,
    CommercialOfferTerms Terms,
    int Revision,
    string Status,
    DateTimeOffset? SentAtUtc,
    DateTimeOffset? OrganizationAcceptedAtUtc,
    DateTimeOffset? ProfessionalAcceptedAtUtc,
    DateTimeOffset? FinalizedAtUtc,
    Guid? OrganizationAcceptedByUserId,
    Guid? ProfessionalAcceptedByUserId,
    string? CancellationReason,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<ProfessionalCommercialOfferRevisionResponse> Revisions);

public sealed record ProfessionalCommercialOfferRevisionResponse(
    int Revision,
    CommercialOfferTerms Terms,
    DateTimeOffset ChangedAtUtc,
    Guid ChangedByUserId);
