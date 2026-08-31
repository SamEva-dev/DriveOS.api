using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;

public sealed record MarketplaceNotificationRequest(
    string RecipientType,
    Guid RecipientId,
    OrganizationId? OrganizationId,
    string Category,
    string TemplateKey,
    string DeduplicationKey,
    IReadOnlyDictionary<string,string?> Parameters,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string? EmailAddress,
    string? CultureCode,
    UserId? ActorUserId);

public interface IMarketplaceNotificationGateway
{
    Task TryEnqueueAsync(MarketplaceNotificationRequest request,CancellationToken ct=default);
}
