using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionOdometerReadingConfiguration : IEntityTypeConfiguration<SessionOdometerReading>
{
    public void Configure(EntityTypeBuilder<SessionOdometerReading> b)
    {
        b.ToTable("session_odometer_readings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionOdometerReadingId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.OdometerKilometers).HasPrecision(12, 1).IsRequired();
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.ObservedAtUtc });
    }
}
