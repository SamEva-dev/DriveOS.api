using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionEnergyEntryConfiguration : IEntityTypeConfiguration<SessionEnergyEntry>
{
    public void Configure(EntityTypeBuilder<SessionEnergyEntry> b)
    {
        b.ToTable("session_energy_entries");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionEnergyEntryId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.EnergyLevelPercent).HasPrecision(5, 1);
        b.Property(x => x.Quantity).HasPrecision(12, 2);
        b.Property(x => x.Note).HasMaxLength(500);
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.ObservedAtUtc });
    }
}
