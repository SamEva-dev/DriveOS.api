using DriveOS.Modules.CurriculumPedagogy.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Configurations;

internal sealed class PedagogicalReadinessDecisionConfiguration
    : IEntityTypeConfiguration<PedagogicalReadinessDecision>
{
    public void Configure(EntityTypeBuilder<PedagogicalReadinessDecision> builder)
    {
        builder.ToTable("pedagogical_readiness_decisions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new PedagogicalReadinessDecisionId(x))
            .ValueGeneratedNever();

        builder.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x));

        builder.Property(x => x.StudentId)
            .HasConversion(x => x.Value, x => new PersonId(x));

        builder.Property(x => x.TrainingPathId)
            .HasConversion(x => x.Value, x => new TrainingPathId(x));

        builder.Property(x => x.ReviewerId)
            .HasConversion(x => x.Value, x => new UserId(x));

        builder.Property(x => x.CreatedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.LastModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);

        builder.Property(x => x.Decision)
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(x => x.Rationale)
            .HasMaxLength(4000);

        builder.Property(x => x.Conditions)
            .HasMaxLength(4000);

        builder.HasIndex(x => new
        {
            x.OrganizationId,
            x.TrainingPathId,
            x.DecidedAtUtc
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
