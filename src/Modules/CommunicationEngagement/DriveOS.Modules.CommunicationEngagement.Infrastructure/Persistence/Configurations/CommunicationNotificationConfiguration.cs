using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Configurations;

internal sealed class CommunicationNotificationConfiguration:IEntityTypeConfiguration<CommunicationNotification>
{
    public void Configure(EntityTypeBuilder<CommunicationNotification>b)
    {
        b.ToTable("notifications");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new CommunicationNotificationId(x)).ValueGeneratedNever();
        b.Property(x=>x.RecipientType).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(
            x=>x==null?(Guid?)null:x.Value.Value,
            x=>x==null?null:new OrganizationId(x.Value));
        b.Property(x=>x.Category).HasMaxLength(80).IsRequired();
        b.Property(x=>x.TemplateKey).HasMaxLength(180).IsRequired();
        b.Property(x=>x.DeduplicationKey).HasMaxLength(180).IsRequired();
        b.Property(x=>x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x=>x.RelatedEntityType).HasMaxLength(80);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.EmailAddress).HasMaxLength(320);
        b.Property(x=>x.CultureCode).HasMaxLength(16);
        b.Property(x=>x.EmailStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x=>x.DeduplicationKey).IsUnique();
        b.HasIndex(x=>new{x.RecipientType,x.RecipientId,x.Status,x.CreatedAtUtc});
        b.Ignore(x=>x.DomainEvents);
    }
}
