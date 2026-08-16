using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class BranchStatusHistoryEntryConfiguration
    : IEntityTypeConfiguration<BranchStatusHistoryEntry>
{
    public void Configure(EntityTypeBuilder<BranchStatusHistoryEntry> builder)
    {
        builder.ToTable("branch_status_history", "organizations");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedNever();

        builder
            .Property(entry => entry.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(branchId => branchId.Value, value => new BranchId(value))
            .IsRequired();

        builder
            .Property(entry => entry.PreviousStatus)
            .HasColumnName("previous_status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder
            .Property(entry => entry.NewStatus)
            .HasColumnName("new_status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder
            .Property(entry => entry.Reason)
            .HasColumnName("reason")
            .HasConversion(reason => reason.Value, value => BranchStatusChangeReason.Create(value))
            .HasMaxLength(BranchStatusChangeReason.MaximumLength)
            .IsRequired();

        builder
            .Property(entry => entry.ChangedByUserId)
            .HasColumnName("changed_by_user_id")
            .IsRequired();

        builder.Property(entry => entry.ChangedAtUtc).HasColumnName("changed_at_utc").IsRequired();

        builder
            .HasIndex(entry => new { entry.BranchId, entry.ChangedAtUtc })
            .HasDatabaseName("ix_branch_status_history_branch_date");
    }
}
