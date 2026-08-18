using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> b)
    {
        b.ToTable("refunds"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new RefundId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.PaymentId).HasColumnName("payment_id").HasConversion(x => x.Value, x => new PaymentId(x)).IsRequired();
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.ProviderReference).HasColumnName("provider_reference").HasMaxLength(250);
        b.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(1000);
        b.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        b.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id").HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x => x.RequestedAtUtc).HasColumnName("requested_at_utc").IsRequired();
        b.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x => x.ApprovedAtUtc).HasColumnName("approved_at_utc"); b.Property(x => x.ProcessingAtUtc).HasColumnName("processing_at_utc");
        b.Property(x => x.CompletedByUserId).HasColumnName("completed_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc"); b.Property(x => x.RejectedAtUtc).HasColumnName("rejected_at_utc"); b.Property(x => x.FailedAtUtc).HasColumnName("failed_at_utc"); b.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Ignore(x => x.DomainEvents);
        b.HasIndex(x => new { x.OrganizationId, x.PaymentId, x.Status }).HasDatabaseName("ix_refunds_org_payment_status");
        b.HasIndex(x => new { x.OrganizationId, x.ProviderReference }).IsUnique().HasFilter("provider_reference IS NOT NULL").HasDatabaseName("ux_refunds_org_provider_reference");
    }
}
