using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamFailureAnalysisConfiguration : IEntityTypeConfiguration<ExamFailureAnalysis>
{
    public void Configure(EntityTypeBuilder<ExamFailureAnalysis> builder)
    {
        builder.ToTable("exam_failure_analyses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamFailureAnalysisId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ExamResultId).HasColumnName("exam_result_id").HasConversion(x => x.Value, x => new ExamResultId(x));
        builder.Property(x => x.ResultRevision).HasColumnName("result_revision");
        builder.Property(x => x.AttemptId).HasColumnName("attempt_id").HasConversion(x => x.Value, x => new ExamAttemptId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.InstructorAnalysis).HasColumnName("instructor_analysis").HasMaxLength(8000);
        builder.Property(x => x.StudentFeedback).HasColumnName("student_feedback").HasMaxLength(8000);
        builder.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(8000);
        builder.Property(x => x.Recommendation).HasColumnName("recommendation").HasMaxLength(8000);
        builder.Property(x => x.TrainingPathId).HasColumnName("training_path_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingPathId(x.Value) : null);
        builder.Property(x => x.OfficialFailureReasonsSnapshot).HasColumnName("official_failure_reasons_snapshot").HasMaxLength(8000);
        builder.Property(x => x.AffectedCompetencyIdsSerialized).HasColumnName("affected_competency_ids").HasMaxLength(8000);
        builder.Property(x => x.FactualEvidence).HasColumnName("factual_evidence").HasMaxLength(8000);
        builder.Property(x => x.ProbableCauseCodesSerialized).HasColumnName("probable_cause_codes").HasMaxLength(4000);
        builder.Property(x => x.Hypotheses).HasColumnName("hypotheses").HasMaxLength(8000);
        builder.Property(x => x.RecommendationCodesSerialized).HasColumnName("recommendation_codes").HasMaxLength(4000);
        builder.Property(x => x.RecommendedHours).HasColumnName("recommended_hours");
        builder.Property(x => x.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder.Property(x => x.SubmittedByUserId).HasColumnName("submitted_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.ApprovedAtUtc).HasColumnName("approved_at_utc");
        builder.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.CompletedByUserId).HasColumnName("completed_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.ExamResultId, x.ResultRevision }).IsUnique().HasDatabaseName("ux_exam_failure_analysis_result_revision");
        builder.OwnsMany(x => x.Findings, finding =>
        {
            finding.ToTable("exam_failure_findings"); finding.WithOwner().HasForeignKey("failure_analysis_id"); finding.HasKey(x => x.Id);
            finding.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); finding.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(64);
            finding.Property(x => x.Code).HasColumnName("code").HasMaxLength(200); finding.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(4000);
            finding.Property(x => x.Critical).HasColumnName("critical"); finding.Property(x => x.Source).HasColumnName("source").HasMaxLength(200);
            finding.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasConversion(x => x.Value, x => new UserId(x)); finding.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            finding.HasIndex("failure_analysis_id", nameof(ExamFailureFinding.Kind), nameof(ExamFailureFinding.Code)).IsUnique();
        });
        builder.Ignore(x => x.DomainEvents);
    }
}
