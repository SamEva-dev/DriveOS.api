using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamPlaceWatchSubscriptionConfiguration : IEntityTypeConfiguration<ExamPlaceWatchSubscription>
{
    public void Configure(EntityTypeBuilder<ExamPlaceWatchSubscription> builder)
    {
        builder.ToTable("exam_place_watch_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamPlaceWatchSubscriptionId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(8).IsRequired();
        builder.Property(x => x.AdministrativeAreaCode).HasColumnName("administrative_area_code").HasMaxLength(64);
        builder.Property(x => x.ExamCategory).HasColumnName("exam_category").HasMaxLength(32);
        builder.Property(x => x.WindowFromUtc).HasColumnName("window_from_utc");
        builder.Property(x => x.WindowToUtc).HasColumnName("window_to_utc");
        builder.Property(x => x.CheckIntervalMinutes).HasColumnName("check_interval_minutes");
        builder.Property(x => x.CenterExternalIds).HasColumnName("center_external_ids").HasMaxLength(4000);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.NextCheckAtUtc).HasColumnName("next_check_at_utc");
        builder.Property(x => x.LastCheckedAtUtc).HasColumnName("last_checked_at_utc");
        builder.Property(x => x.LastSuccessfulCheckAtUtc).HasColumnName("last_successful_check_at_utc");
        builder.Property(x => x.LastAvailabilityDetectedAtUtc).HasColumnName("last_availability_detected_at_utc");
        builder.Property(x => x.LastErrorCode).HasColumnName("last_error_code").HasMaxLength(200);
        builder.Property(x => x.ConsecutiveFailureCount).HasColumnName("consecutive_failure_count");
        builder.Property(x => x.ProcessingLeaseToken).HasColumnName("processing_lease_token");
        builder.Property(x => x.ProcessingLeaseUntilUtc).HasColumnName("processing_lease_until_utc");
        builder.Property(x => x.Version).HasColumnName("xmin").IsRowVersion();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.Status, x.NextCheckAtUtc }).HasDatabaseName("ix_exam_place_watch_due");
        builder.HasIndex(x => new { x.OrganizationId, x.ProviderCode }).HasDatabaseName("ix_exam_place_watch_provider");
        builder.Ignore(x => x.DomainEvents);
    }
}
