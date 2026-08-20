using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new BookingId(x));

        builder.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .IsRequired();

        builder.Property(x => x.BranchId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new BranchId(x.Value) : null);

        builder.Property(x => x.BookingType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.StartAtUtc).IsRequired();
        builder.Property(x => x.EndAtUtc).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreationIdempotencyKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CreationRequestFingerprint).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TrainingPathId);
        builder.Property(x => x.TrainingCategory).HasMaxLength(80);
        builder.Property(x => x.Objectives).HasMaxLength(2000);
        builder.Property(x => x.MeetingPoint).HasMaxLength(500);
        builder.Property(x => x.PricingReference).HasMaxLength(200);
        builder.Property(x => x.TrainingCreditAccountId);
        builder.Property(x => x.CreditQuantity).HasPrecision(18, 2);
        builder.Property(x => x.CreditReservationStatus).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.CreditReservationReference).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.NotificationPolicy).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.HoldExpiresAtUtc);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CancellationReason).HasMaxLength(500);

        builder.Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.HasMany(x => x.Resources)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Resources).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.RescheduleHistory)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.RescheduleHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Cancellations)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Cancellations).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.AttendanceHistory)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InstructorReplacementHistory)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.VehicleReplacementHistory)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.AttendanceHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.InstructorReplacementHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.VehicleReplacementHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.OrganizationId, x.CreationIdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.StartAtUtc, x.EndAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.BranchId, x.Status, x.StartAtUtc });
    }
}
