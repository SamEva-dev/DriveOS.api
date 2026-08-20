using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class AvailabilityExceptionConfiguration : IEntityTypeConfiguration<AvailabilityException>
{
    public void Configure(EntityTypeBuilder<AvailabilityException> builder)
    {
        builder.ToTable("availability_exceptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new AvailabilityExceptionId(x));
        builder.Property(x => x.AvailabilityPlanId).HasConversion(x => x.Value, x => new AvailabilityPlanId(x)).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => new { x.AvailabilityPlanId, x.Date, x.StartTime, x.EndTime });
    }
}
