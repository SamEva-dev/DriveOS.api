using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new PaymentId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.PayerPersonId).HasColumnName("payer_person_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PersonId(x.Value) : null);
        b.Property(x => x.PayerOrganizationId).HasColumnName("payer_organization_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new OrganizationId(x.Value) : null);
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(80).IsRequired();
        b.Property(x => x.ExternalReference).HasColumnName("external_reference").HasMaxLength(250);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(1000);
        b.Property(x => x.ProcessingAtUtc).HasColumnName("processing_at_utc");
        b.Property(x => x.PaidAtUtc).HasColumnName("paid_at_utc");
        b.Property(x => x.FailedAtUtc).HasColumnName("failed_at_utc");
        b.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        b.Property(x => x.RefundedAmount).HasColumnName("refunded_amount").HasPrecision(18, 2);
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Ignore(x => x.AllocatedAmount);
        b.Ignore(x => x.UnallocatedAmount);
        b.Ignore(x => x.RefundableAmount);
        b.Ignore(x => x.DomainEvents);

        b.HasMany(x => x.Allocations).WithOne().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.OrganizationId, x.BillingAccountId, x.Status }).HasDatabaseName("ix_payments_org_account_status");
        b.HasIndex(x => new { x.OrganizationId, x.ExternalReference }).IsUnique().HasFilter("external_reference IS NOT NULL").HasDatabaseName("ux_payments_org_external_reference");
    }
}

internal sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> b)
    {
        b.ToTable("payment_allocations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new PaymentAllocationId(x)).ValueGeneratedNever();
        b.Property(x => x.PaymentId).HasColumnName("payment_id").HasConversion(x => x.Value, x => new PaymentId(x)).IsRequired();
        b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new InvoiceId(x.Value) : null);
        b.Property(x => x.InstallmentId).HasColumnName("installment_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PaymentInstallmentId(x.Value) : null);
        b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.AllocatedAtUtc).HasColumnName("allocated_at_utc").IsRequired();
        b.Property(x => x.AllocatedByUserId).HasColumnName("allocated_by_user_id").HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.HasIndex(x => x.PaymentId).HasDatabaseName("ix_payment_allocations_payment");
        b.HasIndex(x => x.InvoiceId).HasDatabaseName("ix_payment_allocations_invoice");
        b.HasIndex(x => x.InstallmentId).HasDatabaseName("ix_payment_allocations_installment");
    }
}
