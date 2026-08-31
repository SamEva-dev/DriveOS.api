using DomainRelay.Abstractions;
using DriveOS.Modules.CommunicationEngagement.Application.Conversations;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.Modules.ProfessionalMarketplace.Application.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Integrations.ProfessionalMarketplace;

internal sealed class MarketplaceCommunicationGateway(IMediator mediator):IMarketplaceCommunicationGateway
{
    public async Task<Guid> EnsureConversationAsync(OrganizationId organizationId,string relatedEntityType,Guid relatedEntityId,UserId professionalUserId,UserId actorUserId,CancellationToken ct=default)
    {
        var result=await mediator.Send(new EnsureConversationCommand(
            organizationId,relatedEntityType,relatedEntityId,
            [
                new ConversationParticipant(ConversationParticipantType.Organization,organizationId.Value,null),
                new ConversationParticipant(ConversationParticipantType.User,professionalUserId.Value,null)
            ],actorUserId),ct);
        if(result.IsFailure)throw new InvalidOperationException($"Conversation creation failed: {result.Error.Code}");
        return result.Value.Value;
    }
}
