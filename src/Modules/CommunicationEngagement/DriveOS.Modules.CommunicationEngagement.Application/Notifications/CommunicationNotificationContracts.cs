using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Application.Notifications;

public sealed record EnqueueCommunicationNotificationRequest(
    string RecipientType,
    Guid RecipientId,
    OrganizationId? OrganizationId,
    string Category,
    string TemplateKey,
    string DeduplicationKey,
    IReadOnlyDictionary<string,string?> Parameters,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    string? EmailAddress,
    string? CultureCode,
    UserId? ActorUserId);

public sealed record CommunicationNotificationListItem(
    Guid Id,
    string Category,
    string TemplateKey,
    IReadOnlyDictionary<string,string?> Parameters,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    string Status,
    string EmailStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);

public sealed record NotificationPreferenceResponse(
    string Category,
    bool InAppEnabled,
    bool EmailEnabled);

public interface ICommunicationNotificationWriter
{
    Task<Guid> EnqueueAsync(
        EnqueueCommunicationNotificationRequest request,
        CancellationToken cancellationToken=default);
}

public interface ICommunicationNotificationReadService
{
    Task<IReadOnlyList<CommunicationNotificationListItem>> ListForUserAsync(
        UserId userId,
        int take=50,
        bool unreadOnly=false,
        CancellationToken cancellationToken=default);

    Task<int> CountUnreadAsync(
        UserId userId,
        CancellationToken cancellationToken=default);
}

public sealed record MarkCommunicationNotificationReadCommand(
    CommunicationNotificationId Id,
    UserId UserId):ICommand;

public sealed record DismissCommunicationNotificationCommand(
    CommunicationNotificationId Id,
    UserId UserId):ICommand;

public sealed record UpdateNotificationPreferenceCommand(
    UserId UserId,
    string Category,
    bool InAppEnabled,
    bool EmailEnabled):ICommand;

public sealed record GetNotificationPreferencesQuery(
    UserId UserId):IQuery<IReadOnlyList<NotificationPreferenceResponse>>;
