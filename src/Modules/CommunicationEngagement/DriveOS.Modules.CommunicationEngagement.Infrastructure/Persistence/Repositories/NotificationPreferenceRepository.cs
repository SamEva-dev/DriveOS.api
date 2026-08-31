using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Repositories;

internal sealed class NotificationPreferenceRepository(
    CommunicationEngagementDbContext db):INotificationPreferenceRepository
{
    public Task<NotificationPreference?> GetAsync(
        UserId userId,string category,bool tracking,CancellationToken ct=default)
    {
        category=(category??string.Empty).Trim().ToUpperInvariant();
        IQueryable<NotificationPreference> q=db.NotificationPreferences;
        if(!tracking)q=q.AsNoTracking();
        return q.SingleOrDefaultAsync(x=>x.UserId==userId&&x.Category==category,ct);
    }

    public async Task<IReadOnlyList<NotificationPreference>> ListAsync(
        UserId userId,CancellationToken ct=default)=>
        await db.NotificationPreferences.AsNoTracking()
            .Where(x=>x.UserId==userId)
            .OrderBy(x=>x.Category)
            .ToListAsync(ct);

    public void Add(NotificationPreference preference)=>db.NotificationPreferences.Add(preference);
}
