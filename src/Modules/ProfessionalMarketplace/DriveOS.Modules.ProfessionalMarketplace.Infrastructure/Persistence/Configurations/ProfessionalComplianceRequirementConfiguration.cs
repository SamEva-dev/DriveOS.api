using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
public sealed class ProfessionalComplianceRequirementConfiguration:IEntityTypeConfiguration<ProfessionalComplianceRequirement>
{
    public void Configure(EntityTypeBuilder<ProfessionalComplianceRequirement>b)
    {
        b.ToTable("professional_compliance_requirements");
        b.HasKey(x=>x.Id);b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new(x)).ValueGeneratedNever();
        b.Property(x=>x.RequirementCode).HasMaxLength(100).IsRequired();
        b.Property(x=>x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x=>x.ProfessionalType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x=>x.EvidenceKind).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.EvidenceTypeCode).HasMaxLength(80).IsRequired();
        b.Property(x=>x.ApplicableCategoryCodes).HasColumnType("text[]").IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.CreatedByUserId).HasConversion(
            x=>x.HasValue?x.Value.Value:(Guid?)null,
            x=>x.HasValue?new UserId(x.Value):null);
        b.Property(x=>x.LastModifiedByUserId).HasConversion(
            x=>x.HasValue?x.Value.Value:(Guid?)null,
            x=>x.HasValue?new UserId(x.Value):null);
        b.HasIndex(x=>new{x.RequirementCode,x.CountryCode,x.ProfessionalType,x.Version}).IsUnique();
        b.HasIndex(x=>new{x.CountryCode,x.ProfessionalType,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
