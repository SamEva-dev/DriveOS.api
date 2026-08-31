using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class SupplierPaymentRefundConfiguration:IEntityTypeConfiguration<SupplierPaymentRefund>
{
    public void Configure(EntityTypeBuilder<SupplierPaymentRefund>b)
    {
        b.ToTable("supplier_payment_refunds");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new SupplierPaymentRefundId(x)).ValueGeneratedNever();
        b.Property(x=>x.SupplierInvoiceId).HasConversion(x=>x.Value,x=>new SupplierInvoiceId(x)).IsRequired();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Amount).HasPrecision(18,2);
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Reason).HasMaxLength(512).IsRequired();
        b.Property(x=>x.Method).HasMaxLength(80).IsRequired();
        b.Property(x=>x.ProviderReference).HasMaxLength(250);
        b.HasIndex(x=>new{x.SupplierInvoiceId,x.RefundedAtUtc});
        b.HasIndex(x=>x.ProviderReference);
        b.Ignore(x=>x.DomainEvents);
    }
}
