using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(organization => organization.Id);

        builder
            .Property(organization => organization.Id)
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value)
            )
            .ValueGeneratedNever();

        builder
            .Property(organization => organization.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(organization => organization.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        builder
            .Property(organization => organization.Type)
            .HasColumnName("organization_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(organization => organization.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(organization => organization.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(organization => organization.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                userId => userId.HasValue ? userId.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(organization => organization.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder
            .Property(organization => organization.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                userId => userId.HasValue ? userId.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(organization => organization.ProvisioningExternalUserId)
            .HasColumnName("provisioning_external_user_id")
            .HasConversion(
                userId => userId.HasValue ? userId.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(organization => organization.ProvisioningKey)
            .HasColumnName("provisioning_key")
            .HasMaxLength(200);

        builder
            .HasMany(organization => organization.StatusHistory)
            .WithOne()
            .HasForeignKey(entry => entry.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(organization => organization.StatusHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasIndex(organization => new { organization.CountryCode, organization.LegalName })
            .IsUnique()
            .HasDatabaseName("ux_organizations_country_legal_name");

        builder
            .HasIndex(organization => organization.ProvisioningKey)
            .IsUnique()
            .HasFilter("provisioning_key IS NOT NULL")
            .HasDatabaseName("ux_organizations_provisioning_key");

        builder.Ignore(organization => organization.DomainEvents);
    }
}
