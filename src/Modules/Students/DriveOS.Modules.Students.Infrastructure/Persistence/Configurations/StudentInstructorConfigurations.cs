using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentInstructorPortfolioConfiguration
    : IEntityTypeConfiguration<StudentInstructorPortfolio>
{
    public void Configure(EntityTypeBuilder<StudentInstructorPortfolio> b)
    {
        b.ToTable("student_instructor_portfolios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new StudentInstructorPortfolioId(x))
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
            .HasForeignKey(x => x.StudentInstructorPortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.AccessGrants)
            .WithOne()
            .HasForeignKey(x => x.StudentInstructorPortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(x => x.StudentInstructorPortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class StudentInstructorAssignmentConfiguration
    : IEntityTypeConfiguration<StudentInstructorAssignment>
{
    public void Configure(EntityTypeBuilder<StudentInstructorAssignment> b)
    {
        b.ToTable("student_instructor_assignments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentInstructorPortfolioId)
            .HasColumnName("student_instructor_portfolio_id")
            .HasConversion(x => x.Value, x => new StudentInstructorPortfolioId(x));
        b.Property(x => x.InstructorId)
            .HasColumnName("instructor_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.TrainingCategory).HasColumnName("training_category").HasMaxLength(50);
        b.Property(x => x.MaximumScope).HasColumnName("maximum_scope").HasConversion<int>();
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
            x.StudentInstructorPortfolioId,
            x.Type,
            x.Status,
        });
        b.HasIndex(x => new { x.InstructorId, x.Status });
    }
}

internal sealed class StudentInstructorAccessGrantConfiguration
    : IEntityTypeConfiguration<StudentInstructorAccessGrant>
{
    public void Configure(EntityTypeBuilder<StudentInstructorAccessGrant> b)
    {
        b.ToTable("student_instructor_access_grants");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentInstructorPortfolioId)
            .HasColumnName("student_instructor_portfolio_id")
            .HasConversion(x => x.Value, x => new StudentInstructorPortfolioId(x));
        b.Property(x => x.AssignmentId).HasColumnName("assignment_id");
        b.Property(x => x.InstructorId)
            .HasColumnName("instructor_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.Scope).HasColumnName("scope").HasConversion<int>();
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.GrantedAtUtc).HasColumnName("granted_at_utc");
        b.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at_utc");
        b.Property(x => x.RevokedByUserId)
            .HasColumnName("revoked_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasOne<StudentInstructorAssignment>()
            .WithOne()
            .HasForeignKey<StudentInstructorAccessGrant>(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new
        {
            x.InstructorId,
            x.EffectiveFrom,
            x.EffectiveTo,
        });
        b.HasIndex(x => x.AssignmentId).IsUnique();
    }
}

internal sealed class StudentInstructorHistoryConfiguration
    : IEntityTypeConfiguration<StudentInstructorHistory>
{
    public void Configure(EntityTypeBuilder<StudentInstructorHistory> b)
    {
        b.ToTable("student_instructor_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentInstructorPortfolioId)
            .HasColumnName("student_instructor_portfolio_id")
            .HasConversion(x => x.Value, x => new StudentInstructorPortfolioId(x));
        b.Property(x => x.AssignmentId).HasColumnName("assignment_id");
        b.Property(x => x.Action).HasColumnName("action").HasMaxLength(30);
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        b.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.HasIndex(x => new { x.StudentInstructorPortfolioId, x.OccurredAtUtc });
    }
}
