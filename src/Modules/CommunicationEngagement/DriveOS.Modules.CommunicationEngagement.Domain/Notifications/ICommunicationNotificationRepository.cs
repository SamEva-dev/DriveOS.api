using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
public interface ICommunicationNotificationRepository
{
    Task<CommunicationNotification?> GetAsync(CommunicationNotificationId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey,CancellationToken ct=default);
    Task<IReadOnlyList<CommunicationNotification>> ListForUserAsync(UserId userId,int take,bool unreadOnly,CancellationToken ct=default);
    Task<int> CountUnreadAsync(UserId userId,CancellationToken ct=default);
    void Add(CommunicationNotification notification);
}
