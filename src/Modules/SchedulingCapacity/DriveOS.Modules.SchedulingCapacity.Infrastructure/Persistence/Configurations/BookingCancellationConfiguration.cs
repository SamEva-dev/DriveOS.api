using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingCancellationConfiguration : IEntityTypeConfiguration<BookingCancellation>
{
    public void Configure(EntityTypeBuilder<BookingCancellation> builder)
    {
        builder.ToTable("booking_cancellations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new BookingCancellationId(x)).ValueGeneratedNever();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.Initiator).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.InitiatorId);
        builder.Property(x => x.ReasonCode).HasConversion<string>().HasMaxLength(48).IsRequired();
        builder.Property(x => x.ReasonDetails).HasMaxLength(500);
        builder.Property(x => x.CancelledAtUtc).IsRequired();
        builder.Property(x => x.NoticeDurationMinutes).IsRequired();
        builder.Property(x => x.PolicyCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PolicyVersion).IsRequired();
        builder.Property(x => x.PolicyExplanationKey).HasMaxLength(240).IsRequired();
        builder.Property(x => x.CreditDecision).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FeeDecision).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NotificationDecision).HasConversion<string>().HasMaxLength(48).IsRequired();
        builder.Property(x => x.ReplacementRequired).IsRequired();
        builder.Property(x => x.OverrideApplied).IsRequired();
        builder.Property(x => x.OverrideReason).HasMaxLength(500);
        builder.HasIndex(x => x.BookingId).IsUnique();
        builder.HasIndex(x => new { x.BookingId, x.OperationId }).IsUnique();
    }
}
