using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamPlaceWatchHitConfiguration : IEntityTypeConfiguration<ExamPlaceWatchHit>
{
    public void Configure(EntityTypeBuilder<ExamPlaceWatchHit> builder)
    {
        builder.ToTable("exam_place_watch_hits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamPlaceWatchHitId(x)).ValueGeneratedNever();
        builder.Property(x => x.SubscriptionId).HasColumnName("subscription_id").HasConversion(x => x.Value, x => new ExamPlaceWatchSubscriptionId(x));
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ExamPlaceId).HasColumnName("exam_place_id").HasConversion(x => x.Value, x => new ExamPlaceId(x));
        builder.Property(x => x.FirstDetectedAtUtc).HasColumnName("first_detected_at_utc");
        builder.HasIndex(x => new { x.SubscriptionId, x.ExamPlaceId }).IsUnique().HasDatabaseName("ux_exam_place_watch_hit_once");
        builder.HasIndex(x => new { x.OrganizationId, x.FirstDetectedAtUtc }).HasDatabaseName("ix_exam_place_watch_hit_detected");
    }
}
