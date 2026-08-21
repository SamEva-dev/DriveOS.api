using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionMarkerConfiguration : IEntityTypeConfiguration<SessionMarker>
{
    public void Configure(EntityTypeBuilder<SessionMarker> b)
    {
        b.ToTable("session_markers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionMarkerId(x));
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.CompetencyId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new CompetencyId(x.Value) : null);
        b.Property(x => x.RecordedByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.ShortNote).HasMaxLength(500).IsRequired();
        b.Property(x => x.Latitude).HasPrecision(9, 6);
        b.Property(x => x.Longitude).HasPrecision(9, 6);
        b.HasIndex(x => new { x.TrainingSessionId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.TrainingSessionId, x.OccurredAtUtc });
    }
}
