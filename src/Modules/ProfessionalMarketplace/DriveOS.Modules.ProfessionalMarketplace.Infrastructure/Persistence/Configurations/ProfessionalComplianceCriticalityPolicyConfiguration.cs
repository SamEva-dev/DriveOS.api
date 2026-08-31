using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalComplianceCriticalityPolicyConfiguration
    :IEntityTypeConfiguration<ProfessionalComplianceCriticalityPolicy>
{
    public void Configure(EntityTypeBuilder<ProfessionalComplianceCriticalityPolicy>b)
    {
        b.ToTable("professional_compliance_criticality_policies");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalCompliancePolicyId(x)).ValueGeneratedNever();
        b.Property(x=>x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x=>x.RequirementCode).HasMaxLength(120).IsRequired();
        b.Property(x=>x.Criticality).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.Action).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.HasIndex(x=>new{x.CountryCode,x.RequirementCode,x.Version}).IsUnique();
        b.HasIndex(x=>new{x.CountryCode,x.Status,x.EffectiveFrom});
        b.Ignore(x=>x.DomainEvents);
    }
}
