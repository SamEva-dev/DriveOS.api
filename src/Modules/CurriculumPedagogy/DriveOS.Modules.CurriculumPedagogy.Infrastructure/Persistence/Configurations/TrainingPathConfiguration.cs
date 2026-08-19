using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Configurations;

internal sealed class TrainingPathConfiguration : IEntityTypeConfiguration<TrainingPath>
{
    public void Configure(EntityTypeBuilder<TrainingPath> b)
    {
        b.ToTable("training_paths");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingPathId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.CurriculumVersionId).HasConversion(x => x.Value, x => new CurriculumVersionId(x));
        b.Property(x => x.TrainingMode).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.EstimatedPracticalHours).HasPrecision(8, 2);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.SuspensionReason).HasMaxLength(500);
        b.Property(x => x.CancellationReason).HasMaxLength(500);
        b.Property(x => x.ActivatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.StudentId });
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.CurriculumVersionId });
        b.HasMany(x => x.Milestones).WithOne().HasForeignKey(x => x.TrainingPathId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Milestones).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class TrainingPathMilestoneConfiguration : IEntityTypeConfiguration<TrainingPathMilestone>
{
    public void Configure(EntityTypeBuilder<TrainingPathMilestone> b)
    {
        b.ToTable("training_path_milestones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingPathMilestoneId(x)).ValueGeneratedNever();
        b.Property(x => x.TrainingPathId).HasConversion(x => x.Value, x => new TrainingPathId(x));
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.CompletedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.TrainingPathId, x.Code }).IsUnique();
        b.HasIndex(x => new { x.TrainingPathId, x.Order }).IsUnique();
    }
}
