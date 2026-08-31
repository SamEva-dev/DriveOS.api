using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Messaging;

public enum MarketplaceConversationContextType { Opportunity=1, Application=2, Proposal=3, CommercialOffer=4, Engagement=5 }

public sealed record EnsureMarketplaceConversationCommand(
    OrganizationId OrganizationId,
    MarketplaceConversationContextType ContextType,
    Guid ContextId,
    ProfessionalProfileId? ProfessionalProfileId,
    UserId ActorUserId):ICommand<Guid>;

public interface IMarketplaceCommunicationGateway
{
    Task<Guid> EnsureConversationAsync(
        OrganizationId organizationId,
        string relatedEntityType,
        Guid relatedEntityId,
        UserId professionalUserId,
        UserId actorUserId,
        CancellationToken ct=default);
}
