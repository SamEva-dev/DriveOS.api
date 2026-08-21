using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Consequences;

internal sealed class TrainingSessionCompletionConsequenceConfiguration : IEntityTypeConfiguration<TrainingSessionCompletionConsequenceMessage>
{
    public void Configure(EntityTypeBuilder<TrainingSessionCompletionConsequenceMessage> b)
    {
        b.ToTable("training_session_completion_consequences", TrainingDeliverySchema.Name);
        b.HasKey(x => x.Id);
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new DriveOS.SharedKernel.Identifiers.OrganizationId(x)).IsRequired();
        b.Property(x => x.SessionId).HasColumnName("session_id").HasConversion(x => x.Value, x => new DriveOS.SharedKernel.Identifiers.TrainingSessionId(x)).IsRequired();
        b.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(x => x.LastAttemptAtUtc).HasColumnName("last_attempt_at_utc");
        b.Property(x => x.NextAttemptAtUtc).HasColumnName("next_attempt_at_utc");
        b.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
        b.Property(x => x.LastErrorCode).HasColumnName("last_error_code").HasMaxLength(160);
        b.Property(x => x.LastErrorDetail).HasColumnName("last_error_detail").HasMaxLength(2000);
        b.HasIndex(x => new { x.OrganizationId, x.SessionId, x.Kind }).IsUnique();
        b.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        b.HasIndex(x => new { x.Status, x.LastAttemptAtUtc });
    }
}
