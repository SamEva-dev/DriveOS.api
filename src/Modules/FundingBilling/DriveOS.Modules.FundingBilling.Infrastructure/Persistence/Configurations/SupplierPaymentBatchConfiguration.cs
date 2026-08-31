using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class SupplierPaymentBatchConfiguration:IEntityTypeConfiguration<SupplierPaymentBatch>
{
    public void Configure(EntityTypeBuilder<SupplierPaymentBatch>b)
    {
        b.ToTable("supplier_payment_batches");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new SupplierPaymentBatchId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.PaymentMethod).HasMaxLength(80).IsRequired();
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Reference).HasMaxLength(160);
        b.Property(x=>x.TotalAmount).HasPrecision(18,2);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x=>new{x.OrganizationId,x.ScheduledDate,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
