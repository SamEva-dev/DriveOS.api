using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionReportRevisionConfiguration : IEntityTypeConfiguration<SessionReportRevision>
{
    public void Configure(EntityTypeBuilder<SessionReportRevision> b)
    {
        b.ToTable("session_report_revisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionReportRevisionId(x));
        b.Property(x => x.SessionReportId).HasConversion(x => x.Value, x => new TrainingSessionReportId(x)).IsRequired();
        b.Property(x => x.Scenario).HasConversion<int>().IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.FieldCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.CurrentValue).HasMaxLength(5000).IsRequired();
        b.Property(x => x.ProposedValue).HasMaxLength(5000).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        b.Property(x => x.DecisionReason).HasMaxLength(2000);
        b.Property(x => x.RequestedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.DecidedByUserId).HasConversion(x => x!.Value.Value, x => new UserId(x)).IsRequired(false);
        b.HasIndex(x => x.OperationId).IsUnique();
        b.HasIndex(x => new { x.SessionReportId, x.Status });
    }
}
