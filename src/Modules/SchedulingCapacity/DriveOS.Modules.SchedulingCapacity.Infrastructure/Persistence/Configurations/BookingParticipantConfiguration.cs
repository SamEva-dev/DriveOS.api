using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingParticipantConfiguration : IEntityTypeConfiguration<BookingParticipant>
{
    public void Configure(EntityTypeBuilder<BookingParticipant> builder)
    {
        builder.ToTable("booking_participants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new BookingParticipantId(x));

        builder.Property(x => x.BookingId)
            .HasConversion(x => x.Value, x => new BookingId(x))
            .IsRequired();

        builder.Property(x => x.ParticipantType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.ExternalParticipantId).IsRequired();
        builder.HasIndex(x => new { x.BookingId, x.ParticipantType, x.ExternalParticipantId }).IsUnique();
    }
}
