using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class CalendarResourceConfiguration : IEntityTypeConfiguration<CalendarResource>
{
    public void Configure(EntityTypeBuilder<CalendarResource> builder)
    {
        builder.ToTable("calendar_resources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new CalendarResourceId(x));
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.BranchId).HasConversion(x => x!.Value.Value, x => new BranchId(x));
        builder.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.ExternalResourceId).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.RestrictionReason).HasMaxLength(500);
        builder.Property(x => x.UnavailabilityReason).HasMaxLength(500);
        builder.Property(x => x.CreatedByUserId).HasConversion(x => x!.Value.Value, x => new UserId(x));
        builder.Property(x => x.LastModifiedByUserId).HasConversion(x => x!.Value.Value, x => new UserId(x));
        builder.HasIndex(x => new { x.OrganizationId, x.ResourceType, x.ExternalResourceId }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.BranchId, x.Status });
    }
}
