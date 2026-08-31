using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CommunicationEngagement.Domain.Notifications;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetAsync(UserId userId,string category,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<NotificationPreference>> ListAsync(UserId userId,CancellationToken ct=default);
    void Add(NotificationPreference preference);
}
