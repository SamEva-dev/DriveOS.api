using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.SharedKernel.Identifiers;
using Xunit;
namespace DriveOS.UnitTests.Modules.CommunicationEngagement;
public sealed class ConversationTests
{
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;
    private static readonly UserId Actor=new(Guid.NewGuid());
    [Fact] public void Conversation_requires_organization_and_user_participants()
    {
        var org=new OrganizationId(Guid.NewGuid());
        var r=Conversation.Create(new(Guid.NewGuid()),org,"PROFESSIONAL_ENGAGEMENT",Guid.NewGuid(),[new ConversationParticipant(ConversationParticipantType.Organization,org.Value,null)],ConversationVisibility.ParticipantsOnly,Now,Actor);
        Assert.True(r.IsFailure);
    }
    [Fact] public void Empty_message_without_attachment_is_rejected()
    {
        var r=ConversationMessage.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),Actor,"",[],Now);
        Assert.True(r.IsFailure);
    }
    [Fact] public void Attachments_are_references_not_binary_payloads()
    {
        Guid doc=Guid.NewGuid();
        var r=ConversationMessage.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),Actor,"Voir justificatif",[doc],Now);
        Assert.True(r.IsSuccess);Assert.Contains(doc,r.Value.AttachmentDocumentIds);
    }
}
