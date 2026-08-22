using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamPlaceConfiguration : IEntityTypeConfiguration<ExamPlace>
{
    public void Configure(EntityTypeBuilder<ExamPlace> builder)
    {
        builder.ToTable("exam_places");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamPlaceId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ExamCenterId).HasColumnName("exam_center_id").HasConversion(x => x.Value, x => new ExamCenterId(x));
        builder.Property(x => x.ExamType).HasColumnName("exam_type").HasMaxLength(64).IsRequired();
        builder.Property(x => x.LicenseCategory).HasColumnName("license_category").HasMaxLength(32).IsRequired();
        builder.Property(x => x.StartsAtUtc).HasColumnName("starts_at_utc");
        builder.Property(x => x.EndsAtUtc).HasColumnName("ends_at_utc");
        builder.Property(x => x.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(48);
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalPlaceId).HasColumnName("external_place_id").HasMaxLength(200);
        builder.Property(x => x.LastObservedAtUtc).HasColumnName("last_observed_at_utc");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.HoldToken).HasColumnName("hold_token");
        builder.Property(x => x.HoldExpiresAtUtc).HasColumnName("hold_expires_at_utc");
        builder.Property(x => x.HeldByUserId).HasColumnName("held_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.AssignedStudentId).HasColumnName("assigned_student_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PersonId(x.Value) : null);
        builder.Property(x => x.ExamRegistrationId).HasColumnName("exam_registration_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new ExamRegistrationId(x.Value) : null);
        builder.Property(x => x.Version).HasColumnName("xmin").IsRowVersion();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.StartsAtUtc, x.Status }).HasDatabaseName("ix_exam_place_calendar");
        builder.HasIndex(x => new { x.OrganizationId, x.ExamCenterId, x.StartsAtUtc, x.LicenseCategory }).HasDatabaseName("ix_exam_place_center_slot");
        builder.HasIndex(x => new { x.OrganizationId, x.ProviderCode, x.ExternalPlaceId }).IsUnique().HasFilter("external_place_id IS NOT NULL").HasDatabaseName("ux_exam_place_external");
        builder.Ignore(x => x.DomainEvents);
    }
}
