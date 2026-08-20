using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingAttendanceConfiguration : IEntityTypeConfiguration<BookingAttendance>
{
    public void Configure(EntityTypeBuilder<BookingAttendance> builder)
    {
        builder.ToTable("booking_attendance_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new BookingAttendanceId(x)).ValueGeneratedNever();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.SupersedesAttendanceId).HasConversion(
            x => x.HasValue ? x.Value.Value : (Guid?)null,
            x => x.HasValue ? new BookingAttendanceId(x.Value) : null);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.RecordedAtUtc).IsRequired();
        builder.Property(x => x.RecordedBy).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        builder.Property(x => x.ArrivalTimeUtc);
        builder.Property(x => x.DepartureTimeUtc);
        builder.Property(x => x.DelayMinutes).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.EvidenceDocumentId);
        builder.Property(x => x.ChargeDecision).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.CreditDecision).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.FollowUpAction).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.OverrideApplied).IsRequired();
        builder.Property(x => x.OverrideReason).HasMaxLength(500);
        builder.HasIndex(x => new { x.BookingId, x.OperationId }).IsUnique();
        builder.HasIndex(x => new { x.BookingId, x.RecordedAtUtc });
    }
}
