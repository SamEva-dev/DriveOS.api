using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("availability_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new AvailabilityRuleId(x));
        builder.Property(x => x.AvailabilityPlanId).HasConversion(x => x.Value, x => new AvailabilityPlanId(x)).IsRequired();
        builder.Property(x => x.DayOfWeek).HasConversion<int>();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.BranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.TrainingCategory).HasMaxLength(80);
        builder.Property(x => x.ServiceArea).HasMaxLength(200);
        builder.HasIndex(x => new { x.AvailabilityPlanId, x.DayOfWeek, x.StartTime, x.EndTime, x.Type });
    }
}
