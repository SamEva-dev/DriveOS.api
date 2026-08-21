using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionInterventionConfiguration : IEntityTypeConfiguration<SessionIntervention>
{
    public void Configure(EntityTypeBuilder<SessionIntervention> b)
    {
        b.ToTable("session_interventions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionInterventionId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.Context).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        b.Property(x => x.RelatedCompetencyId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new CompetencyId(x.Value) : null);
        b.Property(x => x.Outcome).HasMaxLength(1000);
        b.Property(x => x.InternalComment).HasMaxLength(2000);
        b.Property(x => x.SharedExplanation).HasMaxLength(1000);
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.OccurredAtUtc });
    }
}
