using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSequences;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationSequenceConfiguration
    : IEntityTypeConfiguration<OrganizationSequence>
{
    public void Configure(EntityTypeBuilder<OrganizationSequence> builder)
    {
        builder.ToTable("organization_sequences");
        builder.HasKey(sequence => sequence.Id);

        builder
            .Property(sequence => sequence.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new OrganizationSequenceId(value))
            .ValueGeneratedNever();

        builder
            .Property(sequence => sequence.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .Property(sequence => sequence.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new BranchId(value.Value) : null
            );

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(sequence => sequence.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Branch>()
            .WithMany()
            .HasForeignKey(sequence => sequence.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(sequence => sequence.Scope)
            .HasColumnName("scope")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(sequence => sequence.Code)
            .HasColumnName("code")
            .HasMaxLength(OrganizationSequence.CodeMaximumLength)
            .IsRequired();

        builder.OwnsOne(
            sequence => sequence.Pattern,
            pattern =>
            {
                pattern
                    .Property(value => value.Value)
                    .HasColumnName("pattern")
                    .HasMaxLength(SequencePattern.MaximumLength)
                    .IsRequired();
            }
        );

        builder.Property(sequence => sequence.Padding).HasColumnName("padding").IsRequired();

        builder.Property(sequence => sequence.NextValue).HasColumnName("next_value").IsRequired();

        builder
            .Property(sequence => sequence.ResetPolicy)
            .HasColumnName("reset_policy")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sequence => sequence.LastResetYear).HasColumnName("last_reset_year");

        builder.Property(sequence => sequence.LastResetMonth).HasColumnName("last_reset_month");

        builder
            .Property(sequence => sequence.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(sequence => sequence.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(sequence => sequence.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(sequence => sequence.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(sequence => sequence.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder
            .Property(sequence => sequence.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .HasIndex(sequence => new { sequence.OrganizationId, sequence.Code })
            .IsUnique()
            .HasFilter("branch_id IS NULL")
            .HasDatabaseName("ux_organization_sequences_organization_code");

        builder
            .HasIndex(sequence => new
            {
                sequence.OrganizationId,
                sequence.BranchId,
                sequence.Code,
            })
            .IsUnique()
            .HasFilter("branch_id IS NOT NULL")
            .HasDatabaseName("ux_organization_sequences_branch_code");

        builder
            .HasIndex(sequence => new
            {
                sequence.OrganizationId,
                sequence.BranchId,
                sequence.Status,
            })
            .HasDatabaseName("ix_organization_sequences_scope_status");

        builder.Ignore(sequence => sequence.DomainEvents);
    }
}
