using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationStatusHistoryEntryConfiguration
    : IEntityTypeConfiguration<OrganizationStatusHistoryEntry>
{
    public void Configure(
        EntityTypeBuilder<OrganizationStatusHistoryEntry> builder)
    {
        builder.ToTable("organization_status_history");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(entry => entry.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value))
            .IsRequired();

        builder.Property(entry => entry.PreviousStatus)
            .HasColumnName("previous_status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entry => entry.NewStatus)
            .HasColumnName("new_status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entry => entry.Reason)
            .HasColumnName("reason")
            .HasConversion(
                reason => reason.Value,
                value => OrganizationStatusChangeReason.Create(value))
            .HasMaxLength(OrganizationStatusChangeReason.MaximumLength)
            .IsRequired();

        builder.Property(entry => entry.ChangedByUserId)
            .HasColumnName("changed_by_user_id")
            .IsRequired();

        builder.Property(entry => entry.ChangedAtUtc)
            .HasColumnName("changed_at_utc")
            .IsRequired();

        builder.HasIndex(entry => new
        {
            entry.OrganizationId,
            entry.ChangedAtUtc,
        })
            .HasDatabaseName("ix_organization_status_history_org_date");
    }
}
