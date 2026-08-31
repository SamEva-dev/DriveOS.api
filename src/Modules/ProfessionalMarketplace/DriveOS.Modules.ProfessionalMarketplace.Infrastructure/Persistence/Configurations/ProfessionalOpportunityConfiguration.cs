using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalOpportunityConfiguration:IEntityTypeConfiguration<ProfessionalOpportunity>
{
    public void Configure(EntityTypeBuilder<ProfessionalOpportunity>b)
    {
        b.ToTable("professional_opportunities");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalOpportunityId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.Title).HasMaxLength(180).IsRequired();
        b.Property(x=>x.Description).HasMaxLength(4000).IsRequired();
        b.Property(x=>x.ProfessionalType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x=>x.TeachingCategoryCodes).HasColumnType("text[]").IsRequired();
        b.Property(x=>x.RequiredLanguageCodes).HasColumnType("text[]").IsRequired();
        b.Property(x=>x.RequiredSpecializationCodes).HasColumnType("text[]").IsRequired();
        b.Property(x=>x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x=>x.AreaCode).HasMaxLength(80);b.Property(x=>x.AreaDisplayName).HasMaxLength(160);
        b.Property(x=>x.Latitude).HasPrecision(8,3);b.Property(x=>x.Longitude).HasPrecision(9,3);
        b.Property(x=>x.EngagementType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x=>x.VehicleProvisionMode).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.BudgetMin).HasPrecision(18,2);b.Property(x=>x.BudgetMax).HasPrecision(18,2);
        b.Property(x=>x.Currency).HasMaxLength(3);b.Property(x=>x.BudgetUnit).HasConversion<string>().HasMaxLength(24);
        var windowsComparer=new ValueComparer<OpportunityTimeWindow[]>(
            (a,c)=>JsonSerializer.Serialize(a,(JsonSerializerOptions?)null)==JsonSerializer.Serialize(c,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null).GetHashCode(),
            v=>JsonSerializer.Deserialize<OpportunityTimeWindow[]>(JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),(JsonSerializerOptions?)null)?? new OpportunityTimeWindow[0]);
        b.Property(x=>x.TimeWindows).HasConversion(
            v=>JsonSerializer.Serialize(v,(JsonSerializerOptions?)null),
            v=>JsonSerializer.Deserialize<OpportunityTimeWindow[]>(v,(JsonSerializerOptions?)null)??new OpportunityTimeWindow[0])
            .HasColumnType("jsonb").Metadata.SetValueComparer(windowsComparer);
        b.HasIndex(x=>new{x.OrganizationId,x.Status});
        b.HasIndex(x=>new{x.CountryCode,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
