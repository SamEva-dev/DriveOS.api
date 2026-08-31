using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ProfessionalCommercialOfferConfiguration:IEntityTypeConfiguration<ProfessionalCommercialOffer>
{
    public void Configure(EntityTypeBuilder<ProfessionalCommercialOffer>b)
    {
        b.ToTable("professional_commercial_offers");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalCommercialOfferId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.ApplicationId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalApplicationId(x.Value));
        b.Property(x=>x.ProposalId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalProposalId(x.Value));
        b.Property(x=>x.OpportunityId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalOpportunityId(x.Value));
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.OrganizationAcceptedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.ProfessionalAcceptedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.CancellationReason).HasMaxLength(512);
        var comparer=new ValueComparer<CommercialOfferTerms>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<CommercialOfferTerms>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)!);
        b.Property(x=>x.Terms).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<CommercialOfferTerms>(v,(JsonSerializerOptions?)null)!)
            .HasColumnType("jsonb").Metadata.SetValueComparer(comparer);
        var historyComparer=new ValueComparer<ProfessionalCommercialOfferRevisionSnapshot[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<ProfessionalCommercialOfferRevisionSnapshot[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)?? Array.Empty<ProfessionalCommercialOfferRevisionSnapshot>());
        b.Property(x=>x.RevisionHistory).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<ProfessionalCommercialOfferRevisionSnapshot[]>(v,(JsonSerializerOptions?)null)?? Array.Empty<ProfessionalCommercialOfferRevisionSnapshot>())
            .HasColumnType("jsonb").Metadata.SetValueComparer(historyComparer);
        b.HasIndex(x=>new{x.OrganizationId,x.Status});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
