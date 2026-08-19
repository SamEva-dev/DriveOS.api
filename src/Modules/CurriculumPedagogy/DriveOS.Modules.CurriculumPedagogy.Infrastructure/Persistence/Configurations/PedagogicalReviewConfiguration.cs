using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Configurations;

internal sealed class PedagogicalReviewConfiguration : IEntityTypeConfiguration<PedagogicalReview>
{
    public void Configure(EntityTypeBuilder<PedagogicalReview> b)
    {
        b.ToTable("pedagogical_reviews"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new PedagogicalReviewId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.TrainingPathId).HasConversion(x => x.Value, x => new TrainingPathId(x));
        b.Property(x => x.ReviewerId).HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.Findings).HasMaxLength(8000);
        b.Property(x => x.Recommendations).HasMaxLength(8000);
        b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.Property(x => x.EstimatedRemainingPracticalHours).HasPrecision(8, 2);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.TrainingPathId, x.RequestedAtUtc });
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.Status });
        b.Ignore(x => x.DomainEvents);
    }
}
