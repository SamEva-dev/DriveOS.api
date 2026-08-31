using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalComplianceWaiverConfiguration
    :IEntityTypeConfiguration<ProfessionalComplianceWaiver>
{
    public void Configure(EntityTypeBuilder<ProfessionalComplianceWaiver>b)
    {
        b.ToTable("professional_compliance_waivers");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ProfessionalComplianceWaiverId(x)).ValueGeneratedNever();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.RequirementCode).HasMaxLength(120).IsRequired();
        b.Property(x=>x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x=>x.Reason).HasMaxLength(1000).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.ApprovedByUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.RevokedByUserId).HasConversion(
            x=>x==null?(Guid?)null:x.Value.Value,
            x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.RevocationReason).HasMaxLength(512);
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.RequirementCode,x.Status,x.ValidUntil});
        b.Ignore(x=>x.DomainEvents);
    }
}
