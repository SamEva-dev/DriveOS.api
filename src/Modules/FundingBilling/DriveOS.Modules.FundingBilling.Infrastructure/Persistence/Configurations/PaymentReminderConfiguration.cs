using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;
internal sealed class PaymentReminderConfiguration : IEntityTypeConfiguration<PaymentReminder>
{
    public void Configure(EntityTypeBuilder<PaymentReminder> b)
    {
        b.ToTable("payment_reminders"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new PaymentReminderId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.TargetType).HasColumnName("target_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.TargetId).HasColumnName("target_id").IsRequired(); b.Property(x => x.DueDate).HasColumnName("due_date").IsRequired();
        b.Property(x => x.OutstandingAmount).HasColumnName("outstanding_amount").HasPrecision(18,2).IsRequired(); b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.SequenceNumber).HasColumnName("sequence_number").IsRequired(); b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.RequestedAtUtc).HasColumnName("requested_at_utc").IsRequired(); b.Property(x => x.SentAtUtc).HasColumnName("sent_at_utc"); b.Property(x => x.EmailMessageId).HasColumnName("email_message_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Ignore(x => x.DomainEvents);
        b.HasIndex(x => new { x.OrganizationId, x.TargetType, x.TargetId, x.Status }).HasDatabaseName("ix_payment_reminders_target_status");
    }
}
