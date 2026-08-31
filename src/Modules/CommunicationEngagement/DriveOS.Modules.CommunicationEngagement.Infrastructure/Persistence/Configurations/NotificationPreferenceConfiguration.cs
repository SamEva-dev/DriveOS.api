using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Configurations;

internal sealed class NotificationPreferenceConfiguration:IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference>b)
    {
        b.ToTable("notification_preferences");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new NotificationPreferenceId(x)).ValueGeneratedNever();
        b.Property(x=>x.UserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.Category).HasMaxLength(80).IsRequired();
        b.HasIndex(x=>new{x.UserId,x.Category}).IsUnique();
        b.Ignore(x=>x.DomainEvents);
    }
}
