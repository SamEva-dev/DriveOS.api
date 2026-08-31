using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ExternalAccessGrantConfiguration:IEntityTypeConfiguration<ExternalAccessGrant>
{
    public void Configure(EntityTypeBuilder<ExternalAccessGrant>b)
    {
        b.ToTable("external_access_grants");
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ExternalAccessGrantId(x)).ValueGeneratedNever();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.ResourceType).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x=>x.Permission).HasMaxLength(80).IsRequired();
        b.Property(x=>x.GrantedByUserId).HasConversion(x=>x.Value,x=>new UserId(x)).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.RevokedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.RevocationReason).HasMaxLength(512);
        b.HasIndex(x=>new{x.EngagementId,x.Status});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.OrganizationId,x.Status});
        b.HasIndex(x=>new{x.ResourceType,x.ResourceId,x.Permission,x.Status});
        b.HasIndex(x=>new{x.EngagementId,x.ResourceType,x.ResourceId,x.Permission,x.Status});
        b.Ignore(x=>x.DomainEvents);
    }
}
