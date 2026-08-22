using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamRegistrationSubmissionConfiguration : IEntityTypeConfiguration<ExamRegistrationSubmission>
{
    public void Configure(EntityTypeBuilder<ExamRegistrationSubmission> builder)
    {
        builder.ToTable("exam_registration_submissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamRegistrationSubmissionId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.RegistrationFileId).HasColumnName("registration_file_id").HasConversion(x => x.Value, x => new ExamRegistrationFileId(x));
        builder.Property(x => x.FileRevisionId).HasColumnName("file_revision_id");
        builder.Property(x => x.FileVersion).HasColumnName("file_version");
        builder.Property(x => x.SubmissionVersion).HasColumnName("submission_version");
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.RequestFingerprint).HasColumnName("request_fingerprint").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ExternalSubmissionId).HasColumnName("external_submission_id").HasMaxLength(250);
        builder.Property(x => x.ExternalRegistrationId).HasColumnName("external_registration_id").HasMaxLength(250);
        builder.Property(x => x.CandidateReference).HasColumnName("candidate_reference").HasMaxLength(200);
        builder.Property(x => x.ProviderResponseCode).HasColumnName("provider_response_code").HasMaxLength(200);
        builder.Property(x => x.ProviderResponseJson).HasColumnName("provider_response_json").HasColumnType("text");
        builder.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(200);
        builder.Property(x => x.ErrorMessageKey).HasColumnName("error_message_key").HasMaxLength(250);
        builder.Property(x => x.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(x => x.RespondedAtUtc).HasColumnName("responded_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);

        builder.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique().HasDatabaseName("ux_exam_registration_submission_operation");
        builder.HasIndex(x => new { x.OrganizationId, x.RegistrationId, x.SubmissionVersion }).IsUnique().HasDatabaseName("ux_exam_registration_submission_version");
        builder.HasIndex(x => new { x.OrganizationId, x.RegistrationId, x.FileRevisionId }).IsUnique().HasDatabaseName("ux_exam_registration_submission_file_revision");
        builder.HasIndex(x => new { x.OrganizationId, x.ExternalRegistrationId }).HasDatabaseName("ix_exam_registration_submission_external_registration");
        builder.Ignore(x => x.IsFinal);
        builder.Ignore(x => x.DomainEvents);
    }
}
