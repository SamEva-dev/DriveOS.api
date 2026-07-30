using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Configurations;

internal sealed class
    BranchManagerAssignmentConfiguration
    : IEntityTypeConfiguration<
        BranchManagerAssignment>
{
    public void Configure(
        EntityTypeBuilder<
            BranchManagerAssignment> builder)
    {
        builder.ToTable(
            "branch_manager_assignments",
            OrganizationsSchema.Name);

        builder.HasKey(
            assignment =>
                assignment.Id);

        builder.Property(
                assignment =>
                    assignment.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value =>
                    new BranchManagerAssignmentId(
                        value))
            .ValueGeneratedNever();

        builder.Property(
                assignment =>
                    assignment.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(
                id => id.Value,
                value =>
                    new BranchId(value))
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.ManagerUserId)
            .HasColumnName(
                "manager_user_id")
            .HasConversion(
                id => id.Value,
                value =>
                    new UserId(value))
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.EffectiveFromUtc)
            .HasColumnName(
                "effective_from_utc")
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.EffectiveToUtc)
            .HasColumnName(
                "effective_to_utc");

        builder.Property(
                assignment =>
                    assignment.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.AssignedByUserId)
            .HasColumnName(
                "assigned_by_user_id")
            .HasConversion(
                id => id.Value,
                value =>
                    new UserId(value))
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.AssignedAtUtc)
            .HasColumnName(
                "assigned_at_utc")
            .IsRequired();

        builder.Property(
                assignment =>
                    assignment.EndedByUserId)
            .HasColumnName(
                "ended_by_user_id")
            .HasConversion(
                id =>
                    id.HasValue
                        ? id.Value.Value
                        : (Guid?)null,
                value =>
                    value.HasValue
                        ? new UserId(
                            value.Value)
                        : null);

        builder.Property(
                assignment =>
                    assignment.EndedAtUtc)
            .HasColumnName(
                "ended_at_utc");

        builder.HasIndex(
                assignment =>
                    new
                    {
                        assignment.BranchId,
                        assignment.EffectiveFromUtc,
                    })
            .HasDatabaseName(
                "ix_branch_manager_assignments_branch_date");

        builder.HasIndex(
                assignment =>
                    new
                    {
                        assignment.BranchId,
                        assignment.Status,
                    })
            .HasDatabaseName(
                "ix_branch_manager_assignments_branch_status");

        builder.HasIndex(
                assignment =>
                    assignment.BranchId)
            .IsUnique()
            .HasFilter(
                "status = 'Active' AND effective_to_utc IS NULL")
            .HasDatabaseName(
                "ux_branch_manager_assignments_active_branch");
    }
}