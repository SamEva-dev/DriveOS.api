using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Configurations;

internal sealed class BranchUserAssignmentConfiguration
    : IEntityTypeConfiguration<BranchUserAssignment>
{
    public void Configure(
        EntityTypeBuilder<BranchUserAssignment> builder)
    {
        builder.ToTable(
            "branch_user_assignments",
            OrganizationsSchema.Name);

        builder.HasKey(
            assignment => assignment.Id);

        builder.Property(
                assignment => assignment.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new BranchUserAssignmentId(value))
            .ValueGeneratedNever();

        builder.Property(
                assignment => assignment.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(
                id => id.Value,
                value => new OrganizationId(value))
            .IsRequired();

        builder.Property(
                assignment => assignment.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(
                id => id.Value,
                value => new BranchId(value))
            .IsRequired();

        builder.Property(
                assignment => assignment.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(
                assignment => assignment.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(
                assignment => assignment.AssignmentType)
            .HasColumnName("assignment_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(
                assignment => assignment.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(
                assignment => assignment.StartsAtUtc)
            .HasColumnName("starts_at_utc")
            .IsRequired();

        builder.Property(
                assignment => assignment.PlannedEndAtUtc)
            .HasColumnName("planned_end_at_utc");

        builder.Property(
                assignment => assignment.EffectiveEndAtUtc)
            .HasColumnName("effective_end_at_utc");

        builder.Property(
                assignment => assignment.SuspensionReason)
            .HasColumnName("suspension_reason")
            .HasMaxLength(
                BranchAssignmentReason.MaximumLength);

        builder.Property(
                assignment => assignment.SuspendedAtUtc)
            .HasColumnName("suspended_at_utc");

        builder.Property(
                assignment => assignment.SuspendedByUserId)
            .HasColumnName("suspended_by_user_id")
            .HasConversion(
                id => id.HasValue
                    ? id.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new UserId(value.Value)
                    : null);

        builder.Property(
                assignment => assignment.EndReason)
            .HasColumnName("end_reason")
            .HasMaxLength(
                BranchAssignmentReason.MaximumLength);

        builder.Property(
                assignment => assignment.EndedAtUtc)
            .HasColumnName("ended_at_utc");

        builder.Property(
                assignment => assignment.EndedByUserId)
            .HasColumnName("ended_by_user_id")
            .HasConversion(
                id => id.HasValue
                    ? id.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new UserId(value.Value)
                    : null);

        builder.Property(
                assignment => assignment.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(
                assignment => assignment.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue
                    ? id.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new UserId(value.Value)
                    : null);

        builder.Property(
                assignment => assignment.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder.Property(
                assignment => assignment.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue
                    ? id.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new UserId(value.Value)
                    : null);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(
                assignment => assignment.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(
                assignment => new
                {
                    assignment.OrganizationId,
                    assignment.BranchId,
                    assignment.UserId,
                    assignment.Role,
                })
            .IsUnique()
            .HasFilter("status <> 'Ended'")
            .HasDatabaseName(
                "ux_branch_user_assignments_open_role");

        builder.HasIndex(
                assignment => new
                {
                    assignment.OrganizationId,
                    assignment.UserId,
                    assignment.AssignmentType,
                })
            .IsUnique()
            .HasFilter(
                "assignment_type = 'Primary' AND status <> 'Ended'")
            .HasDatabaseName(
                "ux_branch_user_assignments_primary_user");

        builder.HasIndex(
                assignment => new
                {
                    assignment.OrganizationId,
                    assignment.BranchId,
                    assignment.Status,
                })
            .HasDatabaseName(
                "ix_branch_user_assignments_branch_status");

        builder.HasIndex(
                assignment => new
                {
                    assignment.OrganizationId,
                    assignment.UserId,
                    assignment.Status,
                })
            .HasDatabaseName(
                "ix_branch_user_assignments_user_status");

        builder.Ignore(
            assignment => assignment.DomainEvents);
    }
}