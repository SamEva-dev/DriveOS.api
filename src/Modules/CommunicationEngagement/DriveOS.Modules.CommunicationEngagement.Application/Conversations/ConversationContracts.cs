using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Application.Conversations;

public sealed record EnsureConversationCommand(
    OrganizationId OrganizationId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    ConversationParticipant[] Participants,
    UserId ActorUserId) : ICommand<ConversationId>;

public sealed record SendConversationMessageCommand(
    ConversationId ConversationId,
    OrganizationId OrganizationId,
    UserId SenderUserId,
    Guid[] AttachmentDocumentIds,
    string Body) : ICommand<ConversationMessageId>;

public sealed record MarkConversationReadCommand(
    ConversationId ConversationId,
    OrganizationId OrganizationId,
    ConversationParticipantType ParticipantType,
    Guid PrincipalId,
    UserId ActorUserId) : ICommand;

public sealed record GetConversationThreadQuery(
    ConversationId ConversationId,
    OrganizationId OrganizationId,
    ConversationParticipantType ParticipantType,
    Guid PrincipalId,
    int Take = 100) : IQuery<ConversationThreadResponse>;

public sealed record ConversationThreadResponse(
    Guid ConversationId,
    string RelatedEntityType,
    Guid RelatedEntityId,
    string Status,
    int UnreadCount,
    ConversationMessageResponse[] Messages);

public sealed record ConversationMessageResponse(
    Guid MessageId,
    Guid SenderUserId,
    string Body,
    Guid[] AttachmentDocumentIds,
    DateTimeOffset SentAtUtc);
