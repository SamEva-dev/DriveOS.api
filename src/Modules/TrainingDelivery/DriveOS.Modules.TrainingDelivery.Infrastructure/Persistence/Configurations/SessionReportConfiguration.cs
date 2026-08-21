using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionReportConfiguration : IEntityTypeConfiguration<SessionReport>
{
    public void Configure(EntityTypeBuilder<SessionReport> b)
    {
        b.ToTable("session_reports");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionReportId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.CompletedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.LastSavedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.Version).IsRequired();
        b.Property(x => x.LastCompletedStep).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(5000).IsRequired();
        b.Property(x => x.ObjectivesWorked).HasMaxLength(4000);
        b.Property(x => x.ObjectivesAchieved).HasMaxLength(4000);
        b.Property(x => x.NextObjective).HasMaxLength(2000);
        b.Property(x => x.InstructorComments).HasMaxLength(5000);
        b.Property(x => x.SharedComment).HasMaxLength(5000);
        b.Property(x => x.InternalNote).HasMaxLength(5000);
        b.Property(x => x.DistanceKilometers).HasPrecision(10, 2);
        b.Property(x => x.CorrectedDeliveredDurationMinutes);
        b.HasMany(x => x.NarrativeRevisions)
            .WithOne()
            .HasForeignKey(x => x.SessionReportId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.NarrativeRevisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.SessionReportId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.TrainingSessionId).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
    }
}
