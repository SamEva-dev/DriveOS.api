using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamReadinessOpinionConfiguration : IEntityTypeConfiguration<ExamReadinessOpinion>
{
    public void Configure(EntityTypeBuilder<ExamReadinessOpinion> builder)
    {
        builder.ToTable("exam_readiness_opinions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id")
            .HasConversion(x => x.Value, x => new ExamReadinessOpinionId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.TrainingPathId).HasColumnName("training_path_id")
            .HasConversion(x => x.Value, x => new TrainingPathId(x));
        builder.Property(x => x.PreviousOpinionId).HasColumnName("previous_opinion_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new ExamReadinessOpinionId(x.Value) : null);
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.Opinion).HasColumnName("opinion").HasConversion<string>().HasMaxLength(48);
        builder.Property(x => x.ObservedAutonomy).HasColumnName("observed_autonomy").HasConversion<string>().HasMaxLength(48);
        builder.Property(x => x.ReservationCodesSerialized).HasColumnName("reservation_codes").HasMaxLength(1000);
        builder.Property(x => x.Reservations).HasColumnName("reservations").HasMaxLength(4000);
        builder.Property(x => x.Conditions).HasColumnName("conditions").HasMaxLength(4000);
        builder.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(4000);
        builder.Property(x => x.ProgressPercent).HasColumnName("progress_percent").HasPrecision(5, 2);
        builder.Property(x => x.RequiredCompetencies).HasColumnName("required_competencies");
        builder.Property(x => x.EvaluatedRequiredCompetencies).HasColumnName("evaluated_required_competencies");
        builder.Property(x => x.HasCompletedPedagogicalReview).HasColumnName("has_completed_pedagogical_review");
        builder.Property(x => x.LatestPedagogicalDecision).HasColumnName("latest_pedagogical_decision").HasMaxLength(64);
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.RequestFingerprint).HasColumnName("request_fingerprint").HasMaxLength(64);
        builder.Property(x => x.AuthorId).HasColumnName("author_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        builder.Property(x => x.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);

        builder.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique()
            .HasDatabaseName("ux_exam_readiness_opinion_operation");
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.TrainingPathId, x.AuthorId, x.Version }).IsUnique()
            .HasDatabaseName("ux_exam_readiness_opinion_author_version");
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.TrainingPathId, x.SubmittedAtUtc })
            .HasDatabaseName("ix_exam_readiness_opinion_timeline");

        builder.Ignore(x => x.ReservationCodes);
        builder.Ignore(x => x.DomainEvents);
    }
}
