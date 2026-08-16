using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class InternalTransferCaseConfiguration
    : IEntityTypeConfiguration<InternalTransferCase>
{
    public void Configure(EntityTypeBuilder<InternalTransferCase> b)
    {
        b.ToTable("internal_transfer_cases");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new InternalTransferCaseId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.SourceBranchId)
            .HasColumnName("source_branch_id")
            .HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.TargetBranchId)
            .HasColumnName("target_branch_id")
            .HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Elements).HasColumnName("elements").HasConversion<int>();
        b.Property(x => x.EffectiveOn).HasColumnName("effective_on");
        b.Property(x => x.TemporaryUntil).HasColumnName("temporary_until");
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(1000);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.AnalyzedAtUtc).HasColumnName("analyzed_at_utc");
        b.Property(x => x.AnalysisExpiresAtUtc).HasColumnName("analysis_expires_at_utc");
        b.Property(x => x.AnalyzedByUserId)
            .HasColumnName("analyzed_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.ValidatedAtUtc).HasColumnName("validated_at_utc");
        b.Property(x => x.ValidatedByUserId)
            .HasColumnName("validated_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasOne<DriveOS.Modules.Students.Domain.Students.Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Impacts)
            .WithOne()
            .HasForeignKey(x => x.InternalTransferCaseId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new
        {
            x.OrganizationId,
            x.StudentId,
            x.Status,
        });
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class InternalTransferImpactConfiguration
    : IEntityTypeConfiguration<InternalTransferImpact>
{
    public void Configure(EntityTypeBuilder<InternalTransferImpact> b)
    {
        b.ToTable("internal_transfer_impacts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.InternalTransferCaseId).HasColumnName("internal_transfer_case_id").HasConversion(x => x.Value, x => new InternalTransferCaseId(x));
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.AffectedCount).HasColumnName("affected_count");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.MessageKey).HasColumnName("message_key").HasMaxLength(200);
        b.Property(x => x.RequiresAction).HasColumnName("requires_action");
        b.HasIndex(x => new { x.InternalTransferCaseId, x.Type }).IsUnique();
    }
}
