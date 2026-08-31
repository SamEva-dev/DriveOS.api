using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ProfessionalApplicationConfiguration:IEntityTypeConfiguration<ProfessionalApplication>
{
    public void Configure(EntityTypeBuilder<ProfessionalApplication>b)
    {
        b.ToTable("professional_applications");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalApplicationId(x)).ValueGeneratedNever();
        b.Property(x=>x.OpportunityId).HasConversion(x=>x.Value,x=>new ProfessionalOpportunityId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.Message).HasMaxLength(2000).IsRequired();
        b.Property(x=>x.ProposedRate).HasPrecision(18,2);b.Property(x=>x.Currency).HasMaxLength(3);b.Property(x=>x.RateUnit).HasConversion<string>().HasMaxLength(24);
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();b.Property(x=>x.DecisionReason).HasMaxLength(512);
        b.HasIndex(x=>new{x.OpportunityId,x.ProfessionalProfileId}).IsUnique();
        b.HasIndex(x=>new{x.OrganizationId,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
