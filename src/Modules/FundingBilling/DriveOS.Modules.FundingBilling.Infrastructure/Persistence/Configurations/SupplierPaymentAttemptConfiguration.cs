using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class SupplierPaymentAttemptConfiguration:IEntityTypeConfiguration<SupplierPaymentAttempt>
{
    public void Configure(EntityTypeBuilder<SupplierPaymentAttempt>b)
    {
        b.ToTable("supplier_payment_attempts");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new SupplierPaymentAttemptId(x)).ValueGeneratedNever();
        b.Property(x=>x.SupplierInvoiceId).HasConversion(x=>x.Value,x=>new SupplierInvoiceId(x)).IsRequired();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Amount).HasPrecision(18,2);
        b.Property(x=>x.SettledAmount).HasPrecision(18,2);
        b.Property(x=>x.ReconciliationDifference).HasPrecision(18,2);
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.PaymentMethod).HasMaxLength(80).IsRequired();
        b.Property(x=>x.BankReference).HasMaxLength(160);
        b.Property(x=>x.ProviderReference).HasMaxLength(250);
        b.Property(x=>x.FailureReason).HasMaxLength(1000);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.ReconciliationStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.BatchId).HasConversion(
            x=>x==null?(Guid?)null:x.Value.Value,
            x=>x==null?null:new SupplierPaymentBatchId(x.Value));

        b.HasIndex(x=>new{x.SupplierInvoiceId,x.CreatedAtUtc});
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.ScheduledDate});
        b.HasIndex(x=>x.ProviderReference);
        b.HasIndex(x=>x.BatchId);

        b.Ignore(x=>x.DomainEvents);
    }
}
