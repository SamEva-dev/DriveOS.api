using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
public static class ConversationErrors
{
    public static readonly Error NotFound=Error.NotFound("Communication.Conversation.NotFound","errors.communication.conversation.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("Communication.Conversation.InvalidIdentifier","errors.communication.conversation.invalidIdentifier");
    public static readonly Error InvalidRelatedEntity=Error.Validation("Communication.Conversation.InvalidRelatedEntity","errors.communication.conversation.invalidRelatedEntity");
    public static readonly Error InvalidParticipants=Error.Validation("Communication.Conversation.InvalidParticipants","errors.communication.conversation.invalidParticipants");
    public static readonly Error ParticipantNotFound=Error.NotFound("Communication.Conversation.ParticipantNotFound","errors.communication.conversation.participantNotFound");
    public static readonly Error Closed=Error.Conflict("Communication.Conversation.Closed","errors.communication.conversation.closed");
    public static readonly Error EmptyMessage=Error.Validation("Communication.Message.Empty","errors.communication.message.empty");
    public static readonly Error MessageTooLong=Error.Validation("Communication.Message.TooLong","errors.communication.message.tooLong");
    public static readonly Error SenderNotAuthorized=Error.Conflict("Communication.Message.SenderNotAuthorized","errors.communication.message.senderNotAuthorized");
}
