using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class PaymentInstallmentConfiguration : IEntityTypeConfiguration<PaymentInstallment>
{
    public void Configure(EntityTypeBuilder<PaymentInstallment> b)
    {
        b.ToTable("payment_installments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new PaymentInstallmentId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.DueDate).HasColumnName("due_date").IsRequired();
        b.Property(x => x.ExpectedAmount).HasColumnName("expected_amount").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasPrecision(18, 2).IsRequired();
        b.Ignore(x => x.RemainingAmount);
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.FinancingPersonId).HasColumnName("financing_person_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PersonId(x.Value) : null);
        b.Property(x => x.FinancingOrganizationId).HasColumnName("financing_organization_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new OrganizationId(x.Value) : null);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.PreviousDueDate).HasColumnName("previous_due_date");
        b.Property(x => x.LastReason).HasColumnName("last_reason").HasMaxLength(1000);
        b.Property(x => x.RescheduledAtUtc).HasColumnName("rescheduled_at_utc");
        b.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        b.Property(x => x.WaivedAtUtc).HasColumnName("waived_at_utc");
        b.Property(x => x.OverdueAtUtc).HasColumnName("overdue_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Ignore(x => x.DomainEvents);

        b.HasIndex(x => new { x.OrganizationId, x.BillingAccountId, x.DueDate })
            .HasDatabaseName("ix_payment_installments_org_account_due_date");
        b.HasIndex(x => new { x.OrganizationId, x.Status, x.DueDate })
            .HasDatabaseName("ix_payment_installments_org_status_due_date");
    }
}
