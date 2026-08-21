using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionReportNarrativeRevisionConfiguration : IEntityTypeConfiguration<SessionReportNarrativeRevision>
{
    public void Configure(EntityTypeBuilder<SessionReportNarrativeRevision> b)
    {
        b.ToTable("session_report_narrative_revisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.SessionReportId).HasConversion(x => x.Value, x => new TrainingSessionReportId(x)).IsRequired();
        b.Property(x => x.Kind).HasConversion<int>().IsRequired();
        b.Property(x => x.ReportVersion).IsRequired();
        b.Property(x => x.Content).HasMaxLength(5000);
        b.Property(x => x.ChangedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.ChangedAtUtc).IsRequired();
        b.HasIndex(x => new { x.SessionReportId, x.Kind, x.ReportVersion });
    }
}
