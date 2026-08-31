using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalEngagementConfiguration:IEntityTypeConfiguration<ProfessionalEngagement>
{
    public void Configure(EntityTypeBuilder<ProfessionalEngagement>b)
    {
        b.ToTable("professional_engagements");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.CommercialOfferId).HasConversion(x=>x.Value,x=>new ProfessionalCommercialOfferId(x)).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.StatusReason).HasMaxLength(512);
        b.Property(x=>x.InternalApprovalPrepared).IsRequired();
        b.Property(x=>x.FirstPaidProfessionalInvoiceId).HasConversion(
            x=>x==null?(Guid?)null:x.Value.Value,
            x=>x==null?null:new ProfessionalInvoiceId(x.Value));
        b.Property(x=>x.ConfirmedPaymentMethod).HasMaxLength(80);


        var comparer=new ValueComparer<CommercialOfferTerms>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<CommercialOfferTerms>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)!);

        b.Property(x=>x.TermsSnapshot).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<CommercialOfferTerms>(v,(JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(comparer);

        b.HasIndex(x=>x.CommercialOfferId).IsUnique();
        b.HasIndex(x=>new{x.OrganizationId,x.Status});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status});
        b.Ignore(x=>x.IsOperationallyReady);
        b.Ignore(x=>x.IsReliableRelationship);
        b.Ignore(x=>x.DomainEvents);
    }
}
