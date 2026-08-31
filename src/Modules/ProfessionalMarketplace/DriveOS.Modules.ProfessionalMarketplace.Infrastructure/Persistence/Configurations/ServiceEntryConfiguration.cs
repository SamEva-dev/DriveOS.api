using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;
internal sealed class ServiceEntryConfiguration:IEntityTypeConfiguration<ServiceEntry>
{
    public void Configure(EntityTypeBuilder<ServiceEntry>b)
    {
        b.ToTable("service_entries");b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasConversion(x=>x.Value,x=>new ServiceEntryId(x)).ValueGeneratedNever();
        b.Property(x=>x.EngagementId).HasConversion(x=>x.Value,x=>new ProfessionalEngagementId(x)).IsRequired();
        b.Property(x=>x.MissionId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new ProfessionalMissionId(x.Value));
        b.Property(x=>x.ProfessionalProfileId).HasConversion(x=>x.Value,x=>new ProfessionalProfileId(x)).IsRequired();
        b.Property(x=>x.OrganizationId).HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BranchId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new BranchId(x.Value));
        b.Property(x=>x.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x=>x.ServiceCode).HasMaxLength(80).IsRequired();
        b.Property(x=>x.UnitRate).HasPrecision(18,2);
        b.Property(x=>x.ExpensesAmount).HasPrecision(18,2);
        b.Property(x=>x.IndemnitiesAmount).HasPrecision(18,2);
        b.Property(x=>x.DiscountAmount).HasPrecision(18,2);
        b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x=>x.Description).HasMaxLength(1000).IsRequired();
        b.Property(x=>x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x=>x.ReviewedByUserId).HasConversion(x=>x==null?(Guid?)null:x.Value.Value,x=>x==null?null:new UserId(x.Value));
        b.Property(x=>x.ReviewReason).HasMaxLength(512);
        b.HasIndex(x=>new{x.EngagementId,x.SourceType,x.SourceId}).IsUnique();
        b.HasIndex(x=>new{x.OrganizationId,x.Status,x.ServiceDate});
        b.HasIndex(x=>new{x.ProfessionalProfileId,x.Status,x.ServiceDate});
        b.Ignore(x=>x.BaseAmount);b.Ignore(x=>x.TotalAmount);b.Ignore(x=>x.DomainEvents);
    }
}
