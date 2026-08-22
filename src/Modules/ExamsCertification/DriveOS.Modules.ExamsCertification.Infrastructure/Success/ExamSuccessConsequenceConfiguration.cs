using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Success;

internal sealed class ExamSuccessConsequenceConfiguration : IEntityTypeConfiguration<ExamSuccessConsequenceMessage>
{
    public void Configure(EntityTypeBuilder<ExamSuccessConsequenceMessage> b)
    {
        b.ToTable("exam_success_consequences", ExamsCertificationSchema.Name);
        b.HasKey(x => x.Id);
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new DriveOS.SharedKernel.Identifiers.OrganizationId(x)).IsRequired();
        b.Property(x => x.ResultId).HasColumnName("result_id").HasConversion(x => x.Value, x => new DriveOS.SharedKernel.Identifiers.ExamResultId(x)).IsRequired();
        b.Property(x => x.ResultRevision).HasColumnName("result_revision").IsRequired();
        b.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(x => x.LastAttemptAtUtc).HasColumnName("last_attempt_at_utc");
        b.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        b.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
        b.Property(x => x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        b.Property(x => x.LastErrorCode).HasColumnName("last_error_code").HasMaxLength(180);
        b.Property(x => x.LastErrorDetail).HasColumnName("last_error_detail").HasMaxLength(2000);
        b.HasIndex(x => new { x.OrganizationId, x.ResultId, x.ResultRevision, x.Kind }).IsUnique();
        b.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        b.HasIndex(x => new { x.Status, x.LastAttemptAtUtc });
    }
}
