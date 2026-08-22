using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamPlaceWatchScanConfiguration : IEntityTypeConfiguration<ExamPlaceWatchScan>
{
    public void Configure(EntityTypeBuilder<ExamPlaceWatchScan> builder)
    {
        builder.ToTable("exam_place_watch_scans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamPlaceWatchScanId(x)).ValueGeneratedNever();
        builder.Property(x => x.SubscriptionId).HasColumnName("subscription_id").HasConversion(x => x.Value, x => new ExamPlaceWatchSubscriptionId(x));
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.IsSuccess).HasColumnName("is_success");
        builder.Property(x => x.ExternalSlotsRead).HasColumnName("external_slots_read");
        builder.Property(x => x.NewAvailabilitiesDetected).HasColumnName("new_availabilities_detected");
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(200);
        builder.HasIndex(x => new { x.OrganizationId, x.SubscriptionId, x.StartedAtUtc }).HasDatabaseName("ix_exam_place_watch_scan_history");
    }
}
