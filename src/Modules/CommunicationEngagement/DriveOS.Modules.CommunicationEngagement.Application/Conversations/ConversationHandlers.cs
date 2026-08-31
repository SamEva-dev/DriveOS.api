using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Application.Conversations;

public sealed class EnsureConversationCommandHandler(
    IConversationRepository conversations,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock) : ICommandHandler<EnsureConversationCommand,ConversationId>
{
    public async Task<Result<ConversationId>> Handle(EnsureConversationCommand c,CancellationToken ct)
    {
        Conversation? existing=await conversations.GetByRelatedEntityAsync(c.OrganizationId,c.RelatedEntityType,c.RelatedEntityId,false,ct);
        if(existing is not null)return Result.Success(existing.Id);
        var created=Conversation.Create(new(Guid.NewGuid()),c.OrganizationId,c.RelatedEntityType,c.RelatedEntityId,c.Participants,ConversationVisibility.ParticipantsOnly,clock.UtcNow,c.ActorUserId);
        if(created.IsFailure)return Result.Failure<ConversationId>(created.Error);
        conversations.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public sealed class SendConversationMessageCommandHandler(
    IConversationRepository conversations,
    IConversationMessageRepository messages,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock) : ICommandHandler<SendConversationMessageCommand,ConversationMessageId>
{
    public async Task<Result<ConversationMessageId>> Handle(SendConversationMessageCommand c,CancellationToken ct)
    {
        Conversation? conversation=await conversations.GetAsync(c.ConversationId,true,ct);
        if(conversation is null||conversation.OrganizationId!=c.OrganizationId)
            return Result.Failure<ConversationMessageId>(ConversationErrors.NotFound);
        bool senderAuthorized=conversation.HasParticipant(ConversationParticipantType.User,c.SenderUserId.Value)
            || conversation.HasParticipant(ConversationParticipantType.Organization,c.OrganizationId.Value);
        if(!senderAuthorized)return Result.Failure<ConversationMessageId>(ConversationErrors.SenderNotAuthorized);
        var created=ConversationMessage.Create(new(Guid.NewGuid()),conversation.Id,c.SenderUserId,c.Body,c.AttachmentDocumentIds,clock.UtcNow);
        if(created.IsFailure)return Result.Failure<ConversationMessageId>(created.Error);
        var marked=conversation.MarkMessageSent(created.Value.SentAtUtc,c.SenderUserId);
        if(marked.IsFailure)return Result.Failure<ConversationMessageId>(marked.Error);
        messages.Add(created.Value);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);
    }
}

public sealed class MarkConversationReadCommandHandler(
    IConversationRepository conversations,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock) : ICommandHandler<MarkConversationReadCommand>
{
    public async Task<Result> Handle(MarkConversationReadCommand c,CancellationToken ct)
    {
        Conversation? conversation=await conversations.GetAsync(c.ConversationId,true,ct);
        if(conversation is null||conversation.OrganizationId!=c.OrganizationId)return Result.Failure(ConversationErrors.NotFound);
        var r=conversation.MarkRead(c.ParticipantType,c.PrincipalId,clock.UtcNow,c.ActorUserId);
        if(r.IsFailure)return r;await uow.CommitAsync(ct);return Result.Success();
    }
}

public sealed class GetConversationThreadQueryHandler(
    IConversationRepository conversations,
    IConversationMessageRepository messages) : IQueryHandler<GetConversationThreadQuery,ConversationThreadResponse>
{
    public async Task<Result<ConversationThreadResponse>> Handle(GetConversationThreadQuery q,CancellationToken ct)
    {
        Conversation? conversation=await conversations.GetAsync(q.ConversationId,false,ct);
        if(conversation is null||conversation.OrganizationId!=q.OrganizationId||!conversation.HasParticipant(q.ParticipantType,q.PrincipalId))
            return Result.Failure<ConversationThreadResponse>(ConversationErrors.NotFound);
        int take=Math.Clamp(q.Take,1,500);
        IReadOnlyList<ConversationMessage> items=await messages.ListAsync(conversation.Id,take,ct);
        DateTimeOffset lastRead=conversation.Participants.First(x=>x.Type==q.ParticipantType&&x.PrincipalId==q.PrincipalId).LastReadAtUtc??DateTimeOffset.MinValue;
        int unread=await messages.CountAfterAsync(conversation.Id,lastRead,ct);
        return Result.Success(new ConversationThreadResponse(conversation.Id.Value,conversation.RelatedEntityType,conversation.RelatedEntityId,conversation.Status.ToString(),unread,
            items.Select(x=>new ConversationMessageResponse(x.Id.Value,x.SenderUserId.Value,x.Body,x.AttachmentDocumentIds,x.SentAtUtc)).ToArray()));
    }
}
