using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentBranchPortfolioConfiguration
    : IEntityTypeConfiguration<StudentBranchPortfolio>
{
    public void Configure(EntityTypeBuilder<StudentBranchPortfolio> b)
    {
        b.ToTable("student_branch_portfolios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new StudentBranchPortfolioId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.HasIndex(x => new { x.OrganizationId, x.StudentId }).IsUnique();
        b.HasOne<DriveOS.Modules.Students.Domain.Students.Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Assignments)
            .WithOne()
            .HasForeignKey(x => x.StudentBranchPortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Analyses)
            .WithOne()
            .HasForeignKey(x => x.StudentBranchPortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class StudentBranchAssignmentConfiguration
    : IEntityTypeConfiguration<StudentBranchAssignment>
{
    public void Configure(EntityTypeBuilder<StudentBranchAssignment> b)
    {
        b.ToTable("student_branch_assignments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentBranchPortfolioId).HasColumnName("student_branch_portfolio_id").HasConversion(x => x.Value, x => new StudentBranchPortfolioId(x));
        b.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ServicesAllowed).HasColumnName("services_allowed").HasConversion<int>();
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.EndedByUserId)
            .HasColumnName("ended_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
        b.HasIndex(x => new
        {
            x.StudentBranchPortfolioId,
            x.Type,
            x.Status,
        });
        b.HasIndex(x => new { x.BranchId, x.Status });
    }
}

internal sealed class PrimaryBranchChangeAnalysisConfiguration
    : IEntityTypeConfiguration<PrimaryBranchChangeAnalysis>
{
    public void Configure(EntityTypeBuilder<PrimaryBranchChangeAnalysis> b)
    {
        b.ToTable("primary_branch_change_analyses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentBranchPortfolioId).HasColumnName("student_branch_portfolio_id").HasConversion(x => x.Value, x => new StudentBranchPortfolioId(x));
        b.Property(x => x.CurrentBranchId)
            .HasColumnName("current_branch_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new BranchId(x.Value) : null
            );
        b.Property(x => x.TargetBranchId)
            .HasColumnName("target_branch_id")
            .HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.AnalyzedAtUtc).HasColumnName("analyzed_at_utc");
        b.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc");
        b.Property(x => x.AnalyzedByUserId)
            .HasColumnName("analyzed_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.AppliedAtUtc).HasColumnName("applied_at_utc");
        b.Property(x => x.AppliedByUserId)
            .HasColumnName("applied_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasMany(x => x.Impacts)
            .WithOne()
            .HasForeignKey(x => x.PrimaryBranchChangeAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.StudentBranchPortfolioId, x.ExpiresAtUtc });
    }
}

internal sealed class BranchChangeImpactConfiguration : IEntityTypeConfiguration<BranchChangeImpact>
{
    public void Configure(EntityTypeBuilder<BranchChangeImpact> b)
    {
        b.ToTable("branch_change_impacts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.PrimaryBranchChangeAnalysisId)
            .HasColumnName("primary_branch_change_analysis_id");
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.AffectedCount).HasColumnName("affected_count");
        b.Property(x => x.MessageKey).HasColumnName("message_key").HasMaxLength(200);
        b.Property(x => x.RequiresAction).HasColumnName("requires_action");
    }
}
