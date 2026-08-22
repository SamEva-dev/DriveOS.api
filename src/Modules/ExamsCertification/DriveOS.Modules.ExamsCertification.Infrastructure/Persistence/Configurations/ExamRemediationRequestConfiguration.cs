using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamRemediationRequestConfiguration : IEntityTypeConfiguration<ExamRemediationRequest>
{
    public void Configure(EntityTypeBuilder<ExamRemediationRequest> builder)
    {
        builder.ToTable("exam_remediation_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamRemediationRequestId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        builder.Property(x => x.FailureAnalysisId).HasColumnName("failure_analysis_id").HasConversion(x => x.Value, x => new ExamFailureAnalysisId(x)).IsRequired();
        builder.Property(x => x.ExamResultId).HasColumnName("exam_result_id").HasConversion(x => x.Value, x => new ExamResultId(x)).IsRequired();
        builder.Property(x => x.ResultRevision).HasColumnName("result_revision").IsRequired();
        builder.Property(x => x.FailedAttemptId).HasColumnName("failed_attempt_id").HasConversion(x => x.Value, x => new ExamAttemptId(x)).IsRequired();
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x)).IsRequired();
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        builder.Property(x => x.FailedAttemptNumber).HasColumnName("failed_attempt_number").IsRequired();
        builder.Property(x => x.TrainingPathId).HasColumnName("training_path_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingPathId(x.Value) : null);
        builder.Property(x => x.AnalysisSummary).HasColumnName("analysis_summary").HasMaxLength(8000).IsRequired();
        builder.Property(x => x.RecommendationSummary).HasColumnName("recommendation_summary").HasMaxLength(8000);
        builder.Property(x => x.AffectedCompetencyIdsSerialized).HasColumnName("affected_competency_ids").HasMaxLength(12000).IsRequired();
        builder.Property(x => x.RecommendationCodesSerialized).HasColumnName("recommendation_codes").HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RecommendedHours).HasColumnName("recommended_hours");
        builder.Property(x => x.ResponsibleUserId).HasColumnName("responsible_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.ReviewDate).HasColumnName("review_date");
        builder.Property(x => x.TargetDate).HasColumnName("target_date");
        builder.Property(x => x.MockExamRequired).HasColumnName("mock_exam_required").IsRequired();
        builder.Property(x => x.FundingReviewRequired).HasColumnName("funding_review_required").IsRequired();
        builder.Property(x => x.PedagogicalRemediationPlanId).HasColumnName("pedagogical_remediation_plan_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new RemediationPlanId(x.Value) : null);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(48).IsRequired();
        builder.Property(x => x.DeferredReasonCode).HasColumnName("deferred_reason_code").HasMaxLength(256);
        builder.Property(x => x.FailureCode).HasColumnName("failure_code").HasMaxLength(256);
        builder.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(1000);
        builder.Property(x => x.ProvisionedAtUtc).HasColumnName("provisioned_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.ValidatedForRePresentationAtUtc).HasColumnName("validated_for_re_presentation_at_utc");
        builder.Property(x => x.ValidatedByUserId).HasColumnName("validated_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.FailureAnalysisId }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.ExamResultId, x.ResultRevision }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId, x.Status });
        builder.HasIndex(x => x.PedagogicalRemediationPlanId);
        builder.Ignore(x => x.AffectedCompetencyIds);
        builder.Ignore(x => x.RecommendationCodes);
        builder.Ignore(x => x.DomainEvents);
    }
}
