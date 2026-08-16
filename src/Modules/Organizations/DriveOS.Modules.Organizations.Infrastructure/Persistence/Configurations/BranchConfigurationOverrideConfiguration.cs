using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfigurationOverrideConfiguration
    : IEntityTypeConfiguration<BranchConfigurationOverride>
{
    public void Configure(EntityTypeBuilder<BranchConfigurationOverride> builder)
    {
        builder.ToTable("branch_configuration_overrides");
        builder.HasKey(branchOverride => branchOverride.Id);

        builder
            .Property(branchOverride => branchOverride.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new BranchConfigurationOverrideId(value))
            .ValueGeneratedNever();

        builder
            .Property(branchOverride => branchOverride.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .Property(branchOverride => branchOverride.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(id => id.Value, value => new BranchId(value))
            .IsRequired();

        builder
            .Property(branchOverride => branchOverride.BaseConfigurationId)
            .HasColumnName("base_configuration_id")
            .HasConversion(id => id.Value, value => new OrganizationConfigurationId(value))
            .IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(branchOverride => branchOverride.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Branch>()
            .WithMany()
            .HasForeignKey(branchOverride => branchOverride.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Domain.OrganizationConfigurations.OrganizationConfiguration>()
            .WithMany()
            .HasForeignKey(branchOverride => branchOverride.BaseConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(branchOverride => branchOverride.VersionNumber)
            .HasColumnName("version_number")
            .IsRequired();

        builder
            .Property(branchOverride => branchOverride.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        builder.OwnsOne(
            branchOverride => branchOverride.Payload,
            payload =>
            {
                payload
                    .Property(value => value.Json)
                    .HasColumnName("override_json")
                    .HasColumnType("jsonb")
                    .IsRequired();
            }
        );

        builder
            .Property(branchOverride => branchOverride.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder
            .Property(branchOverride => branchOverride.EffectiveFromUtc)
            .HasColumnName("effective_from_utc");
        builder
            .Property(branchOverride => branchOverride.EffectiveToUtc)
            .HasColumnName("effective_to_utc");
        builder
            .Property(branchOverride => branchOverride.PublishedAtUtc)
            .HasColumnName("published_at_utc");
        builder
            .Property(branchOverride => branchOverride.PublishedByUserId)
            .HasColumnName("published_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(branchOverride => branchOverride.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(branchOverride => branchOverride.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        builder
            .Property(branchOverride => branchOverride.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );
        builder
            .Property(branchOverride => branchOverride.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");
        builder
            .Property(branchOverride => branchOverride.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .HasIndex(branchOverride => new
            {
                branchOverride.OrganizationId,
                branchOverride.BranchId,
                branchOverride.VersionNumber,
            })
            .IsUnique()
            .HasDatabaseName("ux_branch_configuration_overrides_org_branch_version");

        builder
            .HasIndex(branchOverride => new
            {
                branchOverride.OrganizationId,
                branchOverride.BranchId,
                branchOverride.Status,
                branchOverride.EffectiveFromUtc,
            })
            .HasDatabaseName("ix_branch_configuration_overrides_effective_resolution");

        builder
            .HasIndex(branchOverride => branchOverride.BaseConfigurationId)
            .HasDatabaseName("ix_branch_configuration_overrides_base_configuration");
    }
}
