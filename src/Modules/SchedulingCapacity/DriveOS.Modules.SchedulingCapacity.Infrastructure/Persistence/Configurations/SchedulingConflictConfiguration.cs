using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Configurations;

internal sealed class SchedulingConflictConfiguration : IEntityTypeConfiguration<SchedulingConflict>
{
    public void Configure(EntityTypeBuilder<SchedulingConflict> builder)
    {
        builder.ToTable("scheduling_conflicts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => new SchedulingConflictId(x));
        builder.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.BookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        builder.Property(x => x.CalendarResourceId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new CalendarResourceId(x.Value) : null);
        builder.Property(x => x.ConflictingBookingId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BookingId(x.Value) : null);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CauseKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.Property(x => x.SuggestedActions).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Resolution).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.ResolutionReason).HasMaxLength(1000);
        builder.Property(x => x.ResolvedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.OverrideReason).HasMaxLength(1000);
        builder.Property(x => x.OverrideRisk).HasMaxLength(1000);
        builder.Property(x => x.OverrideApprovedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.Status, x.Priority, x.DetectedAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.BookingId, x.Status });
    }
}
