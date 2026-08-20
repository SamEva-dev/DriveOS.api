using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingVehicleReplacementConfiguration : IEntityTypeConfiguration<BookingVehicleReplacement>
{
    public void Configure(EntityTypeBuilder<BookingVehicleReplacement> builder)
    {
        builder.ToTable("booking_vehicle_replacements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new BookingVehicleReplacementId(x)).ValueGeneratedNever();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.PreviousVehicleId).IsRequired();
        builder.Property(x => x.ReplacementVehicleId).IsRequired();
        builder.Property(x => x.PreviousResourceId).HasConversion(x => x.Value, x => new CalendarResourceId(x)).IsRequired();
        builder.Property(x => x.ReplacementResourceId).HasConversion(x => x.Value, x => new CalendarResourceId(x)).IsRequired();
        builder.Property(x => x.Mode).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.BookingId, x.OperationId }).IsUnique();
        builder.HasIndex(x => new { x.PreviousVehicleId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.ReplacementVehicleId, x.OccurredAtUtc });
    }
}
