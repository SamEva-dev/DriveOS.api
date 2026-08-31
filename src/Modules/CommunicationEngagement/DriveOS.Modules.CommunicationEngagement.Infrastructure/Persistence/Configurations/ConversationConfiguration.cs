using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Configurations;
internal sealed class ConversationConfiguration:IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation>b)
    {
        b.ToTable("conversations");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ConversationId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.RelatedEntityType).HasMaxLength(80).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.Visibility).HasConversion<string>().HasMaxLength(32).IsRequired();
        var comparer=new ValueComparer<ConversationParticipant[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ConversationParticipant[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)?? new ConversationParticipant[0]);
        b.Property(x=>x.Participants).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ConversationParticipant[]>(v,(JsonSerializerOptions?)null)?? new ConversationParticipant[0])
            .HasColumnType("jsonb").Metadata.SetValueComparer(comparer);
        b.HasIndex(x=>new{x.OrganizationId,x.RelatedEntityType,x.RelatedEntityId}).IsUnique();
        b.Ignore(x=>x.DomainEvents);
    }
}

internal sealed class ConversationMessageConfiguration:IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage>b)
    {
        b.ToTable("conversation_messages");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ConversationMessageId(x)).ValueGeneratedNever();
        b.Property(x=>x.ConversationId).HasConversion(x=>x.Value,x=>new ConversationId(x)).IsRequired();
        b.Property(x=>x.SenderUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.Body).HasMaxLength(4000).IsRequired();
        b.Property(x=>x.AttachmentDocumentIds).HasColumnType("uuid[]").IsRequired();
        b.HasIndex(x=>new{x.ConversationId,x.SentAtUtc});
        b.Ignore(x=>x.DomainEvents);
    }
}
