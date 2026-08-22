using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamAttestationConfiguration : IEntityTypeConfiguration<ExamAttestation>
{
    public void Configure(EntityTypeBuilder<ExamAttestation> b)
    {
        b.ToTable("exam_attestations"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasColumnName("id").HasConversion(x=>x.Value,x=>new ExamAttestationId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasColumnName("organization_id").HasConversion(x=>x.Value,x=>new OrganizationId(x));
        b.Property(x=>x.ExamResultId).HasColumnName("exam_result_id").HasConversion(x=>x.Value,x=>new ExamResultId(x));
        b.Property(x=>x.ResultRevision).HasColumnName("result_revision"); b.Property(x=>x.ExamAttemptId).HasColumnName("exam_attempt_id").HasConversion(x=>x.Value,x=>new ExamAttemptId(x));
        b.Property(x=>x.ExamRegistrationId).HasColumnName("exam_registration_id").HasConversion(x=>x.Value,x=>new ExamRegistrationId(x)); b.Property(x=>x.StudentId).HasColumnName("student_id").HasConversion(x=>x.Value,x=>new PersonId(x)); b.Property(x=>x.AttemptNumber).HasColumnName("attempt_number");
        b.Property(x=>x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(48); b.Property(x=>x.Reference).HasColumnName("reference").HasMaxLength(160); b.Property(x=>x.CurrentVersion).HasColumnName("current_version");
        b.Property(x=>x.SupersedesAttestationId).HasColumnName("supersedes_attestation_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new ExamAttestationId(x.Value):null);
        b.Property(x=>x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32); b.Property(x=>x.IssuedAtUtc).HasColumnName("issued_at_utc"); b.Property(x=>x.IssuedByUserId).HasColumnName("issued_by_user_id").HasConversion(x=>x.Value,x=>new UserId(x)); b.Property(x=>x.ExpiresAtUtc).HasColumnName("expires_at_utc");
        b.Property(x=>x.DeliveredAtUtc).HasColumnName("delivered_at_utc"); b.Property(x=>x.DeliveredByUserId).HasColumnName("delivered_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.DeliveryChannel).HasColumnName("delivery_channel").HasConversion<string>().HasMaxLength(32);
        b.Property(x=>x.RevokedAtUtc).HasColumnName("revoked_at_utc"); b.Property(x=>x.RevokedByUserId).HasColumnName("revoked_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.RevocationReasonCode).HasColumnName("revocation_reason_code").HasMaxLength(200); b.Property(x=>x.RevocationNotes).HasColumnName("revocation_notes").HasMaxLength(2000); b.Property(x=>x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        b.Property(x=>x.OperationId).HasColumnName("operation_id"); b.Property(x=>x.RequestFingerprint).HasColumnName("request_fingerprint").HasMaxLength(64); b.Property(x=>x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x=>x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); b.Property(x=>x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.HasMany(x=>x.Revisions).WithOne().HasForeignKey(x=>x.AttestationId).OnDelete(DeleteBehavior.Cascade); b.Navigation(x=>x.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x=>new{x.OrganizationId,x.OperationId}).IsUnique().HasDatabaseName("ux_exam_attestations_operation"); b.HasIndex(x=>new{x.OrganizationId,x.ExamResultId,x.Type}).HasDatabaseName("ix_exam_attestations_result_type"); b.HasIndex(x=>new{x.OrganizationId,x.StudentId,x.IssuedAtUtc}).HasDatabaseName("ix_exam_attestations_student_issued"); b.Ignore(x=>x.DomainEvents);
    }
}

internal sealed class ExamAttestationRevisionConfiguration : IEntityTypeConfiguration<ExamAttestationRevision>
{
    public void Configure(EntityTypeBuilder<ExamAttestationRevision> b)
    {
        b.ToTable("exam_attestation_revisions"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasColumnName("id").HasConversion(x=>x.Value,x=>new ExamAttestationRevisionId(x)).ValueGeneratedNever(); b.Property(x=>x.AttestationId).HasColumnName("attestation_id").HasConversion(x=>x.Value,x=>new ExamAttestationId(x)); b.Property(x=>x.Version).HasColumnName("version"); b.Property(x=>x.TemplateCode).HasColumnName("template_code").HasMaxLength(160); b.Property(x=>x.TemplateVersion).HasColumnName("template_version"); b.Property(x=>x.DocumentId).HasColumnName("document_id").HasConversion(x=>x.Value,x=>new DocumentId(x)); b.Property(x=>x.DocumentSha256).HasColumnName("document_sha256").HasMaxLength(64); b.Property(x=>x.PublicVerificationTokenHash).HasColumnName("public_verification_token_hash").HasMaxLength(64); b.Property(x=>x.SignatureProcessReference).HasColumnName("signature_process_reference").HasMaxLength(512); b.Property(x=>x.SignatureEvidenceHash).HasColumnName("signature_evidence_hash").HasMaxLength(128); b.Property(x=>x.SignedByUserId).HasColumnName("signed_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.SignedAtUtc).HasColumnName("signed_at_utc"); b.Property(x=>x.GeneratedByUserId).HasColumnName("generated_by_user_id").HasConversion(x=>x.Value,x=>new UserId(x)); b.Property(x=>x.GeneratedAtUtc).HasColumnName("generated_at_utc"); b.HasIndex(x=>new{x.AttestationId,x.Version}).IsUnique(); b.HasIndex(x=>x.PublicVerificationTokenHash).IsUnique().HasFilter("public_verification_token_hash IS NOT NULL");
    }
}
