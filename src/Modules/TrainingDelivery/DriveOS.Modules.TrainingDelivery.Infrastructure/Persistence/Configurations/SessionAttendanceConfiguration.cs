using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionAttendanceConfiguration : IEntityTypeConfiguration<SessionAttendance>
{
    public void Configure(EntityTypeBuilder<SessionAttendance> b)
    {
        b.ToTable("session_attendance");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionAttendanceId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.SupersedesAttendanceId).HasConversion(
            x => x.HasValue ? x.Value.Value : (Guid?)null,
            x => x.HasValue ? new TrainingSessionAttendanceId(x.Value) : null);
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(2000);
        b.Property(x => x.OverrideReason).HasMaxLength(2000);
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.Revision }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.RecordedAtUtc });
    }
}
