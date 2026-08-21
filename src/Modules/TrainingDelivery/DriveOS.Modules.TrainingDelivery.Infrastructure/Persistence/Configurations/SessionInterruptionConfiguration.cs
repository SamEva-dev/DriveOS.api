using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionInterruptionConfiguration : IEntityTypeConfiguration<SessionInterruption>
{
    public void Configure(EntityTypeBuilder<SessionInterruption> b)
    {
        b.ToTable("session_interruptions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionInterruptionId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.InterruptedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.ResumedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.TerminatedByCancellationId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new SessionCancellationId(x.Value) : null);
        b.Property(x => x.InterruptRequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.ResumeRequestFingerprint).HasMaxLength(64);
        b.Property(x => x.Description).HasMaxLength(3000);
        b.HasIndex(x => new { x.TrainingSessionId, x.InterruptOperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.ResumeOperationId }).IsUnique().HasFilter("\"ResumeOperationId\" IS NOT NULL");
        b.HasIndex(x => new { x.TrainingSessionId, x.StartedAtUtc });
    }
}
