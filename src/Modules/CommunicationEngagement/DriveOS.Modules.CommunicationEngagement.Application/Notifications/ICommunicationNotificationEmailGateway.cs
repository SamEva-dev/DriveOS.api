namespace DriveOS.Modules.CommunicationEngagement.Application.Notifications;

public sealed record CommunicationNotificationEmailRequest(
    string ToEmail,
    string CultureCode,
    string TemplateKey,
    IReadOnlyDictionary<string,string?> Parameters);

public interface ICommunicationNotificationEmailGateway
{
    Task<Guid?> TryQueueAsync(
        CommunicationNotificationEmailRequest request,
        CancellationToken cancellationToken=default);
}
