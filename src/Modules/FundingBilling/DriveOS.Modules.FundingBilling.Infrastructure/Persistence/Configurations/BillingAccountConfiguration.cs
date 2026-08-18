using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;
internal sealed class BillingAccountConfiguration : IEntityTypeConfiguration<BillingAccount>
{
    public void Configure(EntityTypeBuilder<BillingAccount> b)
    {
        b.ToTable("billing_accounts"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new BillingAccountId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.TotalInvoiced).HasColumnName("total_invoiced").HasPrecision(18,2);
        b.Property(x => x.TotalPaid).HasColumnName("total_paid").HasPrecision(18,2);
        b.Property(x => x.CreditBalance).HasColumnName("credit_balance").HasPrecision(18,2);
        b.Ignore(x => x.OutstandingBalance);
        b.Property(x => x.RestrictionReason).HasColumnName("restriction_reason").HasMaxLength(1000);
        b.Property(x => x.SuspensionReason).HasColumnName("suspension_reason").HasMaxLength(1000);
        b.Property(x => x.ClosureReason).HasColumnName("closure_reason").HasMaxLength(1000);
        b.Property(x => x.RestrictedAtUtc).HasColumnName("restricted_at_utc"); b.Property(x => x.SuspendedAtUtc).HasColumnName("suspended_at_utc"); b.Property(x => x.ReactivatedAtUtc).HasColumnName("reactivated_at_utc"); b.Property(x => x.ClosedAtUtc).HasColumnName("closed_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.StudentId }).IsUnique().HasDatabaseName("ux_billing_accounts_organization_student");
        b.HasIndex(x => new { x.OrganizationId, x.Status }).HasDatabaseName("ix_billing_accounts_organization_status");
        b.Ignore(x => x.DomainEvents);
    }
}
