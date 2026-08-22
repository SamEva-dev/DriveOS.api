using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamRegistrationConfiguration : IEntityTypeConfiguration<ExamRegistration>
{
    public void Configure(EntityTypeBuilder<ExamRegistration> builder)
    {
        builder.ToTable("exam_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamRegistrationId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.TrainingPathId).HasColumnName("training_path_id").HasConversion(x => x.Value, x => new TrainingPathId(x));
        builder.Property(x => x.ReadinessDecisionId).HasColumnName("readiness_decision_id").HasConversion(x => x.Value, x => new ExamReadinessDecisionId(x));
        builder.Property(x => x.ExamPlaceId).HasColumnName("exam_place_id").HasConversion(x => x.Value, x => new ExamPlaceId(x));
        builder.Property(x => x.ExamCenterId).HasColumnName("exam_center_id").HasConversion(x => x.Value, x => new ExamCenterId(x));
        builder.Property(x => x.ExamType).HasColumnName("exam_type").HasMaxLength(64).IsRequired();
        builder.Property(x => x.LicenseCategory).HasColumnName("license_category").HasMaxLength(32).IsRequired();
        builder.Property(x => x.ScheduledStartUtc).HasColumnName("scheduled_start_utc");
        builder.Property(x => x.ScheduledEndUtc).HasColumnName("scheduled_end_utc");
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalPlaceId).HasColumnName("external_place_id").HasMaxLength(200);
        builder.Property(x => x.ExternalRegistrationId).HasColumnName("external_registration_id").HasMaxLength(200);
        builder.Property(x => x.CandidateReference).HasColumnName("candidate_reference").HasMaxLength(200);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.RequestFingerprint).HasColumnName("request_fingerprint").HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique().HasDatabaseName("ux_exam_registration_operation");
        builder.HasIndex(x => new { x.OrganizationId, x.ExamPlaceId }).IsUnique().HasDatabaseName("ux_exam_registration_place");
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.ExamType, x.LicenseCategory })
            .IsUnique()
            .HasFilter("status IN (\'Draft\', \'PlaceAssigned\', \'PendingSubmission\', \'Submitted\', \'Confirmed\', \'CorrectionRequested\')")
            .HasDatabaseName("ux_exam_registration_active_student_exam");
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.ScheduledStartUtc }).HasDatabaseName("ix_exam_registration_student_calendar");
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.DomainEvents);
    }
}
