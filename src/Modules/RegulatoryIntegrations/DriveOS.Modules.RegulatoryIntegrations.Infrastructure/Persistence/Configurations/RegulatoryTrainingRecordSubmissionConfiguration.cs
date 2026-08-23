using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Persistence.Configurations;
internal sealed class RegulatoryTrainingRecordSubmissionConfiguration : IEntityTypeConfiguration<RegulatoryTrainingRecordSubmission>
{
    public void Configure(EntityTypeBuilder<RegulatoryTrainingRecordSubmission> b)
    {
        b.ToTable("training_record_submissions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new RegulatoryTrainingRecordSubmissionId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new(x));
        b.Property(x => x.TrainingPathId).HasConversion(x => x.Value, x => new(x));
        b.Property(x => x.SessionId).HasConversion(x => x.Value, x => new(x));
        b.Property(x => x.CountryCode).HasMaxLength(8).IsRequired();
        b.Property(x => x.ProviderCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.IssuesJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.Revision).HasDefaultValue(1).IsRequired();
        b.Property(x => x.SupersedesSubmissionId).HasConversion(
            x => x.HasValue ? x.Value.Value : (Guid?)null,
            x => x.HasValue ? new RegulatoryTrainingRecordSubmissionId(x.Value) : null);
        b.Property(x => x.ExternalReference).HasMaxLength(256);
        b.Property(x => x.LastErrorCode).HasMaxLength(200);
        b.Property(x => x.LastErrorDetail).HasMaxLength(2000);
        b.HasIndex(x => new { x.ProjectionId, x.ProviderCode, x.Revision }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.SessionId, x.ProviderCode, x.Revision }).IsUnique();
        b.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        b.HasIndex(x => new { x.OrganizationId, x.SessionId });
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.TrainingPathId, x.CreatedAtUtc });
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.Status });
    }
}
