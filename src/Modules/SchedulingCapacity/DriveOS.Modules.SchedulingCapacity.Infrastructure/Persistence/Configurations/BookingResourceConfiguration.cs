using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class BookingResourceConfiguration : IEntityTypeConfiguration<BookingResource>
{
    public void Configure(EntityTypeBuilder<BookingResource> builder)
    {
        builder.ToTable("booking_resources");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new BookingResourceId(x));

        builder.Property(x => x.BookingId)
            .HasConversion(x => x.Value, x => new BookingId(x))
            .IsRequired();

        builder.Property(x => x.CalendarResourceId)
            .HasConversion(x => x.Value, x => new CalendarResourceId(x))
            .IsRequired();

        builder.Property(x => x.Quantity).IsRequired();
        builder.HasIndex(x => new { x.BookingId, x.CalendarResourceId }).IsUnique();
        builder.HasIndex(x => x.CalendarResourceId);
    }
}
