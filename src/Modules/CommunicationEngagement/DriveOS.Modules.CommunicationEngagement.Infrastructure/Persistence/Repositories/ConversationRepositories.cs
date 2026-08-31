using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Repositories;
internal sealed class ConversationRepository(CommunicationEngagementDbContext db):IConversationRepository
{
    public Task<Conversation?> GetAsync(ConversationId id,bool tracking,CancellationToken ct=default)=>tracking?db.Conversations.SingleOrDefaultAsync(x=>x.Id==id,ct):db.Conversations.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task<Conversation?> GetByRelatedEntityAsync(OrganizationId organizationId,string relatedEntityType,Guid relatedEntityId,bool tracking,CancellationToken ct=default)
    {
        string type=(relatedEntityType??string.Empty).Trim().ToUpperInvariant();
        IQueryable<Conversation> q=tracking?db.Conversations:db.Conversations.AsNoTracking();
        return q.SingleOrDefaultAsync(x=>x.OrganizationId==organizationId&&x.RelatedEntityType==type&&x.RelatedEntityId==relatedEntityId,ct);
    }
    public void Add(Conversation x)=>db.Conversations.Add(x);
}
internal sealed class ConversationMessageRepository(CommunicationEngagementDbContext db):IConversationMessageRepository
{
    public async Task<IReadOnlyList<ConversationMessage>> ListAsync(ConversationId id,int take,CancellationToken ct=default)=>await db.ConversationMessages.AsNoTracking().Where(x=>x.ConversationId==id).OrderByDescending(x=>x.SentAtUtc).Take(take).OrderBy(x=>x.SentAtUtc).ToListAsync(ct);
    public Task<int> CountAfterAsync(ConversationId id,DateTimeOffset afterUtc,CancellationToken ct=default)=>db.ConversationMessages.AsNoTracking().CountAsync(x=>x.ConversationId==id&&x.SentAtUtc>afterUtc,ct);
    public void Add(ConversationMessage x)=>db.ConversationMessages.Add(x);
}
