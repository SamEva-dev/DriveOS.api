using DriveOS.Modules.CurriculumPedagogy.Domain.RemediationPlans;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Configurations;

internal sealed class RemediationPlanConfiguration : IEntityTypeConfiguration<RemediationPlan>
{
    public void Configure(EntityTypeBuilder<RemediationPlan> builder)
    {
        builder.ToTable("remediation_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new RemediationPlanId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x));

        builder.Property(x => x.StudentId)
            .HasConversion(x => x.Value, x => new PersonId(x));

        builder.Property(x => x.TrainingPathId)
            .HasConversion(x => x.Value, x => new TrainingPathId(x));

        builder.Property(x => x.ResponsibleUserId)
            .HasConversion(x => x.Value, x => new UserId(x));

        builder.Property(x => x.SourcePedagogicalReviewId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new PedagogicalReviewId(x.Value) : null);

        builder.Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.Recommendation)
            .HasMaxLength(8000);

        builder.Property(x => x.RecommendedPracticalHours)
            .HasPrecision(8, 2);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(1000);

        builder.HasMany(x => x.Targets)
            .WithOne()
            .HasForeignKey("RemediationPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Targets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.TrainingPathId,
            x.Status
        });

        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class RemediationPlanTargetConfiguration
    : IEntityTypeConfiguration<RemediationPlanTarget>
{
    public void Configure(EntityTypeBuilder<RemediationPlanTarget> builder)
    {
        builder.ToTable("remediation_plan_targets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new RemediationPlanTargetId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.CompetencyId)
            .HasConversion(x => x.Value, x => new CompetencyId(x));

        builder.Property(x => x.Objective)
            .HasMaxLength(2000);

        builder.HasIndex("RemediationPlanId", nameof(RemediationPlanTarget.CompetencyId))
            .IsUnique();
    }
}
