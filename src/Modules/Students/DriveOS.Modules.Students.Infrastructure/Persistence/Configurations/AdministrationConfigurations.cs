using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class AdministrativeCaseConfiguration : IEntityTypeConfiguration<AdministrativeCase>
{
    public void Configure(EntityTypeBuilder<AdministrativeCase> b)
    {
        b.ToTable("administrative_cases");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new AdministrativeCaseId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.HasIndex(x => new { x.OrganizationId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("ux_administrative_cases_owner_student");
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Requirements)
            .WithOne()
            .HasForeignKey(x => x.AdministrativeCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Blocks)
            .WithOne()
            .HasForeignKey(x => x.AdministrativeCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Exceptions)
            .WithOne()
            .HasForeignKey(x => x.AdministrativeCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(x => x.AdministrativeCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class AdministrativeRequirementConfiguration
    : IEntityTypeConfiguration<AdministrativeRequirement>
{
    public void Configure(EntityTypeBuilder<AdministrativeRequirement> b)
    {
        b.ToTable("administrative_requirements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.AdministrativeCaseId).HasColumnName("administrative_case_id").HasConversion(x => x.Value, x => new AdministrativeCaseId(x));
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
        b.Property(x => x.LabelKey).HasColumnName("label_key").HasMaxLength(200);
        b.Property(x => x.IsBlocking).HasColumnName("is_blocking");
        b.Property(x => x.DueAtUtc).HasColumnName("due_at_utc");
        b.Property(x => x.PolicySource).HasColumnName("policy_source").HasMaxLength(100);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        b.Property(x => x.DecidedByUserId)
            .HasColumnName("decided_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.DecidedAtUtc).HasColumnName("decided_at_utc");
        b.HasIndex(x => new { x.AdministrativeCaseId, x.Code }).IsUnique();
    }
}

internal sealed class AdministrativeBlockConfiguration
    : IEntityTypeConfiguration<AdministrativeBlock>
{
    public void Configure(EntityTypeBuilder<AdministrativeBlock> b)
    {
        b.ToTable("administrative_blocks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.AdministrativeCaseId).HasColumnName("administrative_case_id").HasConversion(x => x.Value, x => new AdministrativeCaseId(x));
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(500);
        b.Property(x => x.AppliedByUserId)
            .HasColumnName("applied_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.AppliedAtUtc).HasColumnName("applied_at_utc");
        b.Property(x => x.ReleaseReason).HasColumnName("release_reason").HasMaxLength(500);
        b.Property(x => x.ReleasedByUserId)
            .HasColumnName("released_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.ReleasedAtUtc).HasColumnName("released_at_utc");
    }
}

internal sealed class ComplianceExceptionConfiguration
    : IEntityTypeConfiguration<ComplianceException>
{
    public void Configure(EntityTypeBuilder<ComplianceException> b)
    {
        b.ToTable("compliance_exceptions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.AdministrativeCaseId).HasColumnName("administrative_case_id").HasConversion(x => x.Value, x => new AdministrativeCaseId(x));
        b.Property(x => x.RequirementId).HasColumnName("requirement_id");
        b.Property(x => x.RequestReason).HasColumnName("request_reason").HasMaxLength(500);
        b.Property(x => x.RequestedByUserId)
            .HasColumnName("requested_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.RequestedAtUtc).HasColumnName("requested_at_utc");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        b.Property(x => x.DecidedByUserId)
            .HasColumnName("decided_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.DecidedAtUtc).HasColumnName("decided_at_utc");
    }
}

internal sealed class AdministrativeHistoryEntryConfiguration
    : IEntityTypeConfiguration<AdministrativeHistoryEntry>
{
    public void Configure(EntityTypeBuilder<AdministrativeHistoryEntry> b)
    {
        b.ToTable("administrative_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.AdministrativeCaseId).HasColumnName("administrative_case_id").HasConversion(x => x.Value, x => new AdministrativeCaseId(x));
        b.Property(x => x.Action).HasColumnName("action").HasMaxLength(60);
        b.Property(x => x.SubjectId).HasColumnName("subject_id");
        b.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(500);
        b.HasIndex(x => new { x.AdministrativeCaseId, x.OccurredAtUtc });
    }
}
