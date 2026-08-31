using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Repositories;

internal sealed class CommunicationNotificationRepository(
    CommunicationEngagementDbContext db):ICommunicationNotificationRepository
{
    public Task<CommunicationNotification?> GetAsync(
        CommunicationNotificationId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.Notifications.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.Notifications.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<bool> ExistsByDeduplicationKeyAsync(string deduplicationKey,CancellationToken ct=default)=>
        db.Notifications.AsNoTracking().AnyAsync(x=>x.DeduplicationKey==deduplicationKey,ct);

    public async Task<IReadOnlyList<CommunicationNotification>> ListForUserAsync(
        UserId userId,int take,bool unreadOnly,CancellationToken ct=default)
    {
        IQueryable<CommunicationNotification> q=db.Notifications.AsNoTracking()
            .Where(x=>x.RecipientType==CommunicationNotificationRecipientType.User&&
                      x.RecipientId==userId.Value&&x.InAppVisible&&
                      x.Status!=CommunicationNotificationStatus.Dismissed);

        if(unreadOnly)
            q=q.Where(x=>x.Status==CommunicationNotificationStatus.Unread);

        return await q.OrderByDescending(x=>x.CreatedAtUtc)
            .Take(Math.Clamp(take,1,200))
            .ToListAsync(ct);
    }

    public Task<int> CountUnreadAsync(UserId userId,CancellationToken ct=default)=>
        db.Notifications.AsNoTracking().CountAsync(x=>
            x.RecipientType==CommunicationNotificationRecipientType.User&&
            x.RecipientId==userId.Value&&x.InAppVisible&&
            x.Status==CommunicationNotificationStatus.Unread,ct);

    public void Add(CommunicationNotification notification)=>db.Notifications.Add(notification);
}
