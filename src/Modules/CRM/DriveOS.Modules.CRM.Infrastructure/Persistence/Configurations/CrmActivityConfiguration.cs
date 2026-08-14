using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class CrmActivityConfiguration : IEntityTypeConfiguration<CrmActivity>
{
    public void Configure(EntityTypeBuilder<CrmActivity> b)
    {
        b.ToTable("activities", t =>
        {
            t.HasCheckConstraint("ck_activities_duration_minutes",
                "duration_minutes IS NULL OR (duration_minutes >= 0 AND duration_minutes <= 1440)");
            t.HasCheckConstraint("ck_activities_import_metadata",
                "origin <> 'Imported' OR (external_id IS NOT NULL AND idempotency_key IS NOT NULL)");
            t.HasCheckConstraint("ck_activities_failed_sync_error",
                "sync_status <> 'Failed' OR sync_error_key IS NOT NULL");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id")
            .HasConversion(x => x.Value, x => new CrmActivityId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.LeadId).HasColumnName("lead_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new LeadId(x.Value) : null);
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        b.Property(x => x.Details).HasColumnName("details").HasMaxLength(4000);
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.Property(x => x.AdvisorUserId).HasColumnName("advisor_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.InvalidatedAtUtc).HasColumnName("invalidated_at_utc");
        b.Property(x => x.InvalidatedByUserId).HasColumnName("invalidated_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.InvalidationReason).HasColumnName("invalidation_reason").HasMaxLength(500);
        b.OwnsOne(x => x.Metadata, m =>
        {
            m.Property(x => x.Result).HasColumnName("result").HasMaxLength(100);
            m.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            m.Property(x => x.IsInternal).HasColumnName("is_internal");
            m.Property(x => x.IsUnfollowed).HasColumnName("is_unfollowed");
            m.Property(x => x.RequiresRegularization).HasColumnName("requires_regularization");
            m.Property(x => x.Origin).HasColumnName("origin").HasConversion<string>().HasMaxLength(20);
            m.Property(x => x.SyncStatus).HasColumnName("sync_status").HasConversion<string>().HasMaxLength(20);
            m.Property(x => x.ExternalId).HasColumnName("external_id").HasMaxLength(200);
            m.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(200);
            m.Property(x => x.SyncErrorKey).HasColumnName("sync_error_key").HasMaxLength(200);
            m.Property(x => x.SyncAttemptCount).HasColumnName("sync_attempt_count");
            m.Property(x => x.LastSyncAttemptAtUtc).HasColumnName("last_sync_attempt_at_utc");
            m.Property(x => x.AttachmentName).HasColumnName("attachment_name").HasMaxLength(255);
            m.Property(x => x.AttachmentReference).HasColumnName("attachment_reference").HasMaxLength(1000);
        });
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.LeadId, x.OccurredAtUtc })
            .HasDatabaseName("ix_activities_organization_lead_occurred");
        b.HasIndex(x => new { x.OrganizationId, x.OccurredAtUtc })
            .HasDatabaseName("ix_activities_organization_occurred");
        b.HasIndex(x => new { x.OrganizationId, x.AdvisorUserId, x.OccurredAtUtc })
            .HasDatabaseName("ix_activities_organization_advisor_occurred");
        b.Ignore(x => x.DomainEvents);
    }
}
