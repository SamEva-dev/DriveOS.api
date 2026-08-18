using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.ToTable("invoices");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new InvoiceId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.CustomerPersonId).HasColumnName("customer_person_id").HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        b.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(80);
        b.Property(x => x.IssueDate).HasColumnName("issue_date");
        b.Property(x => x.DueDate).HasColumnName("due_date");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.IssuedAtUtc).HasColumnName("issued_at_utc");
        b.Property(x => x.OverdueAtUtc).HasColumnName("overdue_at_utc");
        b.Property(x => x.IssuedByUserId).HasColumnName("issued_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CreditedAmount).HasColumnName("credited_amount").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.PaidAmount).HasColumnName("paid_amount").HasPrecision(18, 2).IsRequired();
        b.Ignore(x => x.CreditableAmount);
        b.Ignore(x => x.RemainingAmount);
        b.Ignore(x => x.Subtotal);
        b.Ignore(x => x.TaxAmount);
        b.Ignore(x => x.TotalAmount);
        b.Ignore(x => x.DomainEvents);

        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(x => new { x.OrganizationId, x.BillingAccountId, x.Status }).HasDatabaseName("ix_invoices_org_account_status");
        b.HasIndex(x => new { x.OrganizationId, x.InvoiceNumber }).IsUnique().HasFilter("invoice_number IS NOT NULL").HasDatabaseName("ux_invoices_org_number");
    }
}

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> b)
    {
        b.ToTable("invoice_lines");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new InvoiceLineId(x)).ValueGeneratedNever();
        b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasConversion(x => x.Value, x => new InvoiceId(x)).IsRequired();
        b.Property(x => x.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        b.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(18, 4);
        b.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(40).IsRequired();
        b.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2);
        b.Property(x => x.TaxRate).HasColumnName("tax_rate").HasPrecision(8, 4);
        b.Property(x => x.NetAmount).HasColumnName("net_amount").HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        b.HasIndex(x => x.InvoiceId).HasDatabaseName("ix_invoice_lines_invoice");
    }
}
