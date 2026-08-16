using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentSessionRevisionConfiguration
    : IEntityTypeConfiguration<AssessmentSessionRevision>
{
    public void Configure(EntityTypeBuilder<AssessmentSessionRevision> builder)
    {
        builder.ToTable("assessment_session_revisions");
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new AssessmentSessionRevisionId(x))
            .ValueGeneratedNever();
        builder
            .Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        builder
            .Property(x => x.SessionId)
            .HasConversion(x => x.Value, x => new AssessmentSessionId(x))
            .HasColumnName("session_id");
        builder.Property(x => x.Revision).HasColumnName("revision");
        builder.Property(x => x.AnswersJson).HasColumnName("answers").HasColumnType("jsonb");
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
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
        builder
            .Property(x => x.SavedByUserId)
            .HasConversion(x => x.Value, x => new UserId(x))
            .HasColumnName("saved_by_user_id");
        builder.Property(x => x.SavedAtUtc).HasColumnName("saved_at_utc");
        builder
            .HasIndex(x => new
            {
                x.OrganizationId,
                x.SessionId,
                x.Revision,
            })
            .IsUnique();
        builder
            .HasOne<AssessmentSession>()
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
