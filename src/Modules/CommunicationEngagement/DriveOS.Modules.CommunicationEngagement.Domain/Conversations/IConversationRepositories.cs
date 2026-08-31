using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
public interface IConversationRepository
{
    Task<Conversation?> GetAsync(ConversationId id,bool tracking,CancellationToken ct=default);
    Task<Conversation?> GetByRelatedEntityAsync(OrganizationId organizationId,string relatedEntityType,Guid relatedEntityId,bool tracking,CancellationToken ct=default);
    void Add(Conversation conversation);
}
public interface IConversationMessageRepository
{
    Task<IReadOnlyList<ConversationMessage>> ListAsync(ConversationId conversationId,int take,CancellationToken ct=default);
    Task<int> CountAfterAsync(ConversationId conversationId,DateTimeOffset afterUtc,CancellationToken ct=default);
    void Add(ConversationMessage message);
}
