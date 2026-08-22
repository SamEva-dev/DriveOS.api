using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamReadinessDecisionConfiguration : IEntityTypeConfiguration<ExamReadinessDecision>
{
    public void Configure(EntityTypeBuilder<ExamReadinessDecision> builder)
    {
        builder.ToTable("exam_readiness_decisions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new ExamReadinessDecisionId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));

        builder.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));

        builder.Property(x => x.TrainingPathId)
            .HasColumnName("training_path_id")
            .HasConversion(x => x.Value, x => new TrainingPathId(x));

        builder.Property(x => x.Version)
            .HasColumnName("version");

        builder.Property(x => x.Outcome)
            .HasColumnName("outcome")
            .HasConversion<string>()
            .HasMaxLength(48);

        builder.Property(x => x.PedagogicalCheck)
            .HasColumnName("pedagogical_check")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.AdministrativeCheck)
            .HasColumnName("administrative_check")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.FinancialCheck)
            .HasColumnName("financial_check")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.RegulatoryCheck)
            .HasColumnName("regulatory_check")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Rationale)
            .HasColumnName("rationale")
            .HasMaxLength(4000);

        builder.Property(x => x.Conditions)
            .HasColumnName("conditions")
            .HasMaxLength(4000);

        builder.Property(x => x.ReviewerId)
            .HasColumnName("reviewer_id")
            .HasConversion(x => x.Value, x => new UserId(x));

        builder.Property(x => x.DecidedAtUtc)
            .HasColumnName("decided_at_utc");

        builder.Property(x => x.IsCurrent)
            .HasColumnName("is_current");

        builder.Property(x => x.SupersededByDecisionId)
            .HasColumnName("superseded_by_decision_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new ExamReadinessDecisionId(x.Value) : null);

        builder.Property(x => x.SupersededAtUtc)
            .HasColumnName("superseded_at_utc");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder.Property(x => x.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.TrainingPathId, x.Version })
            .IsUnique()
            .HasDatabaseName("ux_exam_readiness_decision_version");

        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.TrainingPathId })
            .HasFilter("is_current = TRUE")
            .IsUnique()
            .HasDatabaseName("ux_exam_readiness_current");

        builder.Ignore(x => x.DomainEvents);
    }
}
