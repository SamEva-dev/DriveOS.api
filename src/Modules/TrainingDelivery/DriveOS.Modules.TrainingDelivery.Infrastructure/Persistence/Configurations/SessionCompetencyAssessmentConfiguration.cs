using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionCompetencyAssessmentConfiguration : IEntityTypeConfiguration<SessionCompetencyAssessment>
{
    public void Configure(EntityTypeBuilder<SessionCompetencyAssessment> b)
    {
        b.ToTable("session_competency_assessments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionCompetencyAssessmentId(x)).ValueGeneratedNever();
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.CompetencyId).HasConversion(x => x.Value, x => new CompetencyId(x)).IsRequired();
        b.Property(x => x.CurriculumVersionId).HasConversion(x => x.Value, x => new CurriculumVersionId(x)).IsRequired();
        b.Property(x => x.RelatedInterventionId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingSessionInterventionId(x.Value) : null);
        b.Property(x => x.AssessorUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.LevelCode).HasMaxLength(60).IsRequired();
        b.Property(x => x.ObservedCriteria).HasMaxLength(4000);
        b.Property(x => x.Context).HasMaxLength(4000);
        b.Property(x => x.InternalComment).HasMaxLength(4000);
        b.Property(x => x.SharedComment).HasMaxLength(4000);
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.CompetencyId }).IsUnique();
        b.HasIndex(x => x.PedagogyAssessmentId).IsUnique();
    }
}
