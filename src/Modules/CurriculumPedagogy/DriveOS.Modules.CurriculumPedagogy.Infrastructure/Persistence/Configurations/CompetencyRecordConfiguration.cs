using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Configurations;

internal sealed class CompetencyRecordConfiguration : IEntityTypeConfiguration<CompetencyRecord>
{
    public void Configure(EntityTypeBuilder<CompetencyRecord> b)
    {
        b.ToTable("competency_records");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CompetencyRecordId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.TrainingPathId).HasConversion(x => x.Value, x => new TrainingPathId(x));
        b.Property(x => x.CurriculumVersionId).HasConversion(x => x.Value, x => new CurriculumVersionId(x));
        b.Property(x => x.CompetencyId).HasConversion(x => x.Value, x => new CompetencyId(x));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.TrainingPathId, x.CompetencyId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.CurriculumVersionId });
        b.HasMany(x => x.Assessments).WithOne().HasForeignKey(x => x.CompetencyRecordId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Assessments).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Ignore(x => x.CurrentLevelCode);
        b.Ignore(x => x.LastAssessedAtUtc);
        b.Ignore(x => x.LastAssessorUserId);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class CompetencyAssessmentConfiguration : IEntityTypeConfiguration<CompetencyAssessment>
{
    public void Configure(EntityTypeBuilder<CompetencyAssessment> b)
    {
        b.ToTable("competency_assessments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new CompetencyAssessmentId(x)).ValueGeneratedNever();
        b.Property(x => x.CompetencyRecordId).HasConversion(x => x.Value, x => new CompetencyRecordId(x));
        b.Property(x => x.LevelCode).HasMaxLength(60);
        b.Property(x => x.AssessorUserId).HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.Comment).HasMaxLength(4000);
        b.HasIndex(x => new { x.CompetencyRecordId, x.AssessedAtUtc });
        b.HasIndex(x => new { x.CompetencyRecordId, x.SourceSessionId }).IsUnique();
    }
}
