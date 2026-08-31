using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class SupplierInvoiceConfiguration:IEntityTypeConfiguration<SupplierInvoice>
{
    public void Configure(EntityTypeBuilder<SupplierInvoice>b)
    {
        b.ToTable("supplier_invoices");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new SupplierInvoiceId(x)).ValueGeneratedNever();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.SourceType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x=>x.SupplierReference).HasMaxLength(80);
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Subtotal).HasPrecision(18,2);
        b.Property(x=>x.TaxAmount).HasPrecision(18,2);
        b.Property(x=>x.InvoiceMode).HasMaxLength(32).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x=>x.SettlementStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.PaidAmount).HasPrecision(18,2);
        b.Property(x=>x.RefundedAmount).HasPrecision(18,2);
        b.Property(x=>x.MatchedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.OperationallyApprovedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.FinanciallyApprovedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.ScheduledForPaymentByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.DecisionReason).HasMaxLength(512);

        b.HasIndex(x=>new{x.SourceType,x.ExternalSourceId}).IsUnique();
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.DueDate});
        b.HasIndex(x=>new{x.SupplierOrganizationId,x.Status,x.DueDate});
        b.HasIndex(x=>new{x.SupplierReference,x.SupplierOrganizationId});

        b.Ignore(x=>x.TotalAmount);
        b.Ignore(x=>x.NetPaidAmount);
        b.Ignore(x=>x.RemainingAmount);
        b.Ignore(x=>x.DomainEvents);
    }
}
