using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentStatusBoardConfiguration : IEntityTypeConfiguration<StudentStatusBoard>
{
    public void Configure(EntityTypeBuilder<StudentStatusBoard> b)
    {
        b.ToTable("student_status_boards");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new StudentStatusBoardId(x))
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
        b.Property(x => x.FinancialStatus)
            .HasColumnName("financial_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.PedagogicalStatus)
            .HasColumnName("pedagogical_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.SchedulingStatus)
            .HasColumnName("scheduling_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.ExamStatus)
            .HasColumnName("exam_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.PortalAccessStatus)
            .HasColumnName("portal_access_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Ignore(x => x.DomainEvents);
        b.HasMany(x => x.Blocks)
            .WithOne()
            .HasForeignKey(x => x.StudentStatusBoardId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(x => x.StudentStatusBoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class StudentOperationalBlockConfiguration
    : IEntityTypeConfiguration<StudentOperationalBlock>
{
    public void Configure(EntityTypeBuilder<StudentOperationalBlock> b)
    {
        b.ToTable("student_operational_blocks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentStatusBoardId).HasColumnName("student_status_board_id").HasConversion(x => x.Value, x => new StudentStatusBoardId(x));
        b.Property(x => x.BlockType).HasColumnName("block_type").HasMaxLength(80).IsRequired();
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(1000).IsRequired();
        b.Property(x => x.SourceDomain)
            .HasColumnName("source_domain")
            .HasMaxLength(80)
            .IsRequired();
        b.Property(x => x.BlockingActions).HasColumnName("blocking_actions").HasConversion<int>();
        b.Property(x => x.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.AppliedAtUtc).HasColumnName("applied_at_utc");
        b.Property(x => x.AppliedByUserId)
            .HasColumnName("applied_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.ExpectedResolution)
            .HasColumnName("expected_resolution")
            .HasMaxLength(500);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ResolutionType)
            .HasColumnName("resolution_type")
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(x => x.ResolutionReason).HasColumnName("resolution_reason").HasMaxLength(1000);
        b.Property(x => x.ResolvedAtUtc).HasColumnName("resolved_at_utc");
        b.Property(x => x.ResolvedByUserId)
            .HasColumnName("resolved_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.OverrideUntilUtc).HasColumnName("override_until_utc");
        b.Property(x => x.OverrideReason).HasColumnName("override_reason").HasMaxLength(1000);
        b.Property(x => x.OverrideByUserId)
            .HasColumnName("override_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasIndex(x => new { x.StudentStatusBoardId, x.Status });
    }
}

internal sealed class StudentBlockHistoryConfiguration
    : IEntityTypeConfiguration<StudentBlockHistory>
{
    public void Configure(EntityTypeBuilder<StudentBlockHistory> b)
    {
        b.ToTable("student_block_history");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.StudentStatusBoardId).HasColumnName("student_status_board_id").HasConversion(x => x.Value, x => new StudentStatusBoardId(x));
        b.Property(x => x.BlockId).HasColumnName("block_id");
        b.Property(x => x.Action).HasColumnName("action").HasMaxLength(60).IsRequired();
        b.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(1000);
        b.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.HasIndex(x => new { x.StudentStatusBoardId, x.OccurredAtUtc });
    }
}
