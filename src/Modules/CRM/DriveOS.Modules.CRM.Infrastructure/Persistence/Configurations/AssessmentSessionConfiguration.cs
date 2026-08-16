using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentSessionConfiguration : IEntityTypeConfiguration<AssessmentSession>
{
    public void Configure(EntityTypeBuilder<AssessmentSession> builder)
    {
        builder.ToTable("assessment_sessions");
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasConversion(x => x.Value, x => new AssessmentSessionId(x))
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder
            .Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        builder
            .Property(x => x.AppointmentId)
            .HasConversion(x => x.Value, x => new AssessmentAppointmentId(x))
            .HasColumnName("appointment_id");
        builder
            .Property(x => x.LeadId)
            .HasConversion(x => x.Value, x => new LeadId(x))
            .HasColumnName("lead_id");
        builder
            .Property(x => x.EvaluatorUserId)
            .HasConversion(x => x.Value, x => new UserId(x))
            .HasColumnName("evaluator_user_id");
        builder
            .Property(x => x.QuestionnaireCode)
            .HasColumnName("questionnaire_code")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.QuestionnaireVersion).HasColumnName("questionnaire_version");
        builder
            .Property(x => x.QuestionnaireSnapshotJson)
            .HasColumnName("questionnaire_snapshot")
            .HasColumnType("jsonb")
            .IsRequired();
        builder
            .Property(x => x.AnswersJson)
            .HasColumnName("answers")
            .HasColumnType("jsonb")
            .IsRequired();
        builder
            .Property(x => x.FactualObservations)
            .HasColumnName("factual_observations")
            .HasMaxLength(8000);
        builder
            .Property(x => x.PedagogicalInterpretation)
            .HasColumnName("pedagogical_interpretation")
            .HasMaxLength(8000);
        builder.Property(x => x.Recommendation).HasColumnName("recommendation").HasMaxLength(8000);
        builder.Property(x => x.InternalNotes).HasColumnName("internal_notes").HasMaxLength(8000);
        builder
            .Property(x => x.ProspectComment)
            .HasColumnName("prospect_comment")
            .HasMaxLength(4000);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(x => x.Revision).HasColumnName("revision").IsConcurrencyToken();
        builder.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(x => x.LastSavedAtUtc).HasColumnName("last_saved_at_utc");
        builder.Property(x => x.SubmittedAtUtc).HasColumnName("submitted_at_utc");
        builder
            .Property(x => x.SubmittedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("submitted_by_user_id");
        builder.Property(x => x.ResultJson).HasColumnName("result").HasColumnType("jsonb");
        builder
            .Property(x => x.AiSuggestionJson)
            .HasColumnName("ai_suggestion")
            .HasColumnType("jsonb");
        builder
            .Property(x => x.ResultConfidence)
            .HasColumnName("result_confidence")
            .HasConversion<int?>();
        builder.Property(x => x.ResultStatus).HasColumnName("result_status").HasConversion<int>();
        builder
            .Property(x => x.CorrectionReason)
            .HasColumnName("correction_reason")
            .HasMaxLength(2000);
        builder.Property(x => x.ResultValidatedAtUtc).HasColumnName("result_validated_at_utc");
        builder
            .Property(x => x.ResultValidatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("result_validated_by_user_id");
        builder.Property(x => x.ResultSharedAtUtc).HasColumnName("result_shared_at_utc");
        builder
            .Property(x => x.ResultSharedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("result_shared_by_user_id");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder
            .Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("created_by_user_id");
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder
            .Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("last_modified_by_user_id");
        builder.HasIndex(x => new { x.OrganizationId, x.AppointmentId }).IsUnique();
        builder.HasIndex(x => new { x.OrganizationId, x.LeadId });
        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.EvaluatorUserId,
            x.Status,
        });
        builder.HasIndex(x => new { x.OrganizationId, x.ResultStatus });
        builder
            .HasOne<AssessmentAppointment>()
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
