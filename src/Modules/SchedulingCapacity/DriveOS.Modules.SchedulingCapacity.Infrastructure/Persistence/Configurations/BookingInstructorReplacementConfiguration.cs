using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingInstructorReplacementConfiguration : IEntityTypeConfiguration<BookingInstructorReplacement>
{
    public void Configure(EntityTypeBuilder<BookingInstructorReplacement> builder)
    {
        builder.ToTable("booking_instructor_replacements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new BookingInstructorReplacementId(x)).ValueGeneratedNever();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.OperationId).IsRequired();
        builder.Property(x => x.PreviousInstructorId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        builder.Property(x => x.ReplacementInstructorId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        builder.Property(x => x.PreviousResourceId).HasConversion(x => x.Value, x => new CalendarResourceId(x)).IsRequired();
        builder.Property(x => x.ReplacementResourceId).HasConversion(x => x.Value, x => new CalendarResourceId(x)).IsRequired();
        builder.Property(x => x.Mode).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.Property(x => x.AccessExpiresAtUtc);
        builder.HasIndex(x => new { x.BookingId, x.OperationId }).IsUnique();
        builder.HasIndex(x => new { x.PreviousInstructorId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.ReplacementInstructorId, x.OccurredAtUtc });
    }
}
