using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class AvailabilityPlanConfiguration : IEntityTypeConfiguration<AvailabilityPlan>
{
    public void Configure(EntityTypeBuilder<AvailabilityPlan> builder)
    {
        builder.ToTable("availability_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new AvailabilityPlanId(x));
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.CalendarResourceId).HasConversion(x => x.Value, x => new CalendarResourceId(x)).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.PreferredMeetingPoint).HasMaxLength(500);
        builder.Property(x => x.MaximumTravelDistanceKm).HasPrecision(8, 2);
        builder.Property(x => x.PreferredInstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.CreatedByUserId).HasConversion(x => x!.Value.Value, x => new UserId(x));
        builder.Property(x => x.LastModifiedByUserId).HasConversion(x => x!.Value.Value, x => new UserId(x));
        builder.HasIndex(x => new { x.OrganizationId, x.CalendarResourceId, x.Status });
        builder.HasMany(x => x.Rules).WithOne().HasForeignKey(x => x.AvailabilityPlanId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Exceptions).WithOne().HasForeignKey(x => x.AvailabilityPlanId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Rules).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Exceptions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
