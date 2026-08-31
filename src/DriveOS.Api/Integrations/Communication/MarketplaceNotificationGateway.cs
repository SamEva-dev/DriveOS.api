using DriveOS.Modules.CommunicationEngagement.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;

namespace DriveOS.Api.Integrations.Communication;

internal sealed class MarketplaceNotificationGateway(
    ICommunicationNotificationWriter writer,
    ILogger<MarketplaceNotificationGateway> logger):IMarketplaceNotificationGateway
{
    public async Task TryEnqueueAsync(MarketplaceNotificationRequest request,CancellationToken ct=default)
    {
        try
        {
            await writer.EnqueueAsync(new(
                request.RecipientType,request.RecipientId,request.OrganizationId,request.Category,
                request.TemplateKey,request.DeduplicationKey,request.Parameters,
                request.RelatedEntityType,request.RelatedEntityId,
                request.EmailAddress,request.CultureCode,request.ActorUserId),ct);
        }
        catch(Exception ex)
        {
            // Notification delivery must never roll back the source business transition.
            // DeduplicationKey makes an explicit retry safe.
            logger.LogError(ex,"Unable to enqueue marketplace notification {DeduplicationKey}",request.DeduplicationKey);
        }
    }
}
