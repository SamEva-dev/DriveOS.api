using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Conversations;

/// <summary>
/// Message persisted by BC-15. Attachments are references to BC-06 documents; no binary is stored here.
/// </summary>
public sealed class ConversationMessage : AggregateRoot<ConversationMessageId>, IAuditableEntity
{
    private ConversationMessage() { }

    private ConversationMessage(
        ConversationMessageId id,
        ConversationId conversationId,
        UserId senderUserId,
        string body,
        Guid[] attachmentDocumentIds,
        DateTimeOffset sentAtUtc) : base(id)
    {
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Body = body;
        AttachmentDocumentIds = attachmentDocumentIds;
        SentAtUtc = sentAtUtc.ToUniversalTime();
    }

    public ConversationId ConversationId { get; private set; }
    public UserId SenderUserId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public Guid[] AttachmentDocumentIds { get; private set; } = [];
    public DateTimeOffset SentAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ConversationMessage> Create(
        ConversationMessageId id,
        ConversationId conversationId,
        UserId senderUserId,
        string body,
        IEnumerable<Guid>? attachmentDocumentIds,
        DateTimeOffset now)
    {
        if (id.IsEmpty || conversationId.IsEmpty || senderUserId.IsEmpty)
            return Result.Failure<ConversationMessage>(ConversationErrors.InvalidIdentifier);
        string normalized = (body ?? string.Empty).Trim();
        Guid[] attachments = (attachmentDocumentIds ?? []).Where(x => x != Guid.Empty).Distinct().Take(20).ToArray();
        if (normalized.Length == 0 && attachments.Length == 0)
            return Result.Failure<ConversationMessage>(ConversationErrors.EmptyMessage);
        if (normalized.Length > 4000)
            return Result.Failure<ConversationMessage>(ConversationErrors.MessageTooLong);

        var message = new ConversationMessage(id, conversationId, senderUserId, normalized, attachments, now);
        message.SetCreatedAudit(now, senderUserId);
        return Result.Success(message);
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? actor) { CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = actor; }
    public void SetModifiedAudit(DateTimeOffset at, UserId? actor) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = actor; }
}
