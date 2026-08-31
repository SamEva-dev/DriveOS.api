using DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ProfessionalInvoiceConfiguration:IEntityTypeConfiguration<ProfessionalInvoice>
{
    public void Configure(EntityTypeBuilder<ProfessionalInvoice>b)
    {
        b.ToTable("professional_invoices");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalInvoiceId(x)).ValueGeneratedNever();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.ServiceStatementId).HasConversion(x=>x.Value,x=>new ServiceStatementId(x)).IsRequired();
        b.Property(x=>x.ClientOrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Mode).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.InvoiceNumber).HasMaxLength(80);
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Subtotal).HasPrecision(18,2);
        b.Property(x=>x.TaxAmount).HasPrecision(18,2);
        b.Property(x=>x.BankReference).HasMaxLength(160);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.PaymentStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.FinanceSupplierInvoiceStatus).HasMaxLength(40);
        b.HasIndex(x=>x.FinanceSupplierInvoiceId).IsUnique();
        b.Property(x=>x.ValidatedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.HasIndex(x=>x.ServiceStatementId).IsUnique();
        b.HasIndex(x=>new{x.ClientOrganizationId,x.Status,x.DueDate});
        b.HasIndex(x=>new{x.ProviderOrganizationId,x.Status,x.DueDate});
        b.HasIndex(x=>new{x.InvoiceNumber,x.ProviderOrganizationId});
        b.Ignore(x=>x.Total);b.Ignore(x=>x.DomainEvents);
    }
}
