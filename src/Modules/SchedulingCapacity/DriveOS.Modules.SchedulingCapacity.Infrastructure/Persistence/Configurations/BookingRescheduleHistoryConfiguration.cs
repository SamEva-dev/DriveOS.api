using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingRescheduleHistoryConfiguration : IEntityTypeConfiguration<BookingRescheduleHistory>
{
    public void Configure(EntityTypeBuilder<BookingRescheduleHistory> builder)
    {
        builder.ToTable("booking_reschedule_history");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new BookingRescheduleId(x)).ValueGeneratedNever();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.PreviousStartAtUtc).IsRequired();
        builder.Property(x => x.PreviousEndAtUtc).IsRequired();
        builder.Property(x => x.NewStartAtUtc).IsRequired();
        builder.Property(x => x.NewEndAtUtc).IsRequired();
        builder.Property(x => x.PreviousBranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.NewBranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ResourcesChanged).IsRequired();
        builder.Property(x => x.PreviousResourceFingerprint).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.NewResourceFingerprint).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();

        builder.HasIndex(x => new { x.BookingId, x.OperationId }).IsUnique();
        builder.HasIndex(x => new { x.BookingId, x.OccurredAtUtc });
    }
}
