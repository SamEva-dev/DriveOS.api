using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;

internal sealed class TrainingSessionCancellationConsequenceConfiguration : IEntityTypeConfiguration<TrainingSessionCancellationConsequenceMessage>
{
    public void Configure(EntityTypeBuilder<TrainingSessionCancellationConsequenceMessage> b)
    {
        b.ToTable("training_session_cancellation_consequences", TrainingDeliverySchema.Name);
        b.HasKey(x => x.Id);
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.SessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x));
        b.Property(x => x.CancellationId).HasConversion(x => x.Value, x => new SessionCancellationId(x));
        b.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.LastErrorCode).HasMaxLength(200);
        b.Property(x => x.LastErrorDetail).HasMaxLength(2000);
        b.HasIndex(x => new { x.OrganizationId, x.CancellationId, x.Kind }).IsUnique();
        b.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        b.HasIndex(x => new { x.Status, x.LastAttemptAtUtc });
    }
}
