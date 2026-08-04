using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfigurationConfiguration :
    IEntityTypeConfiguration<Domain.OrganizationConfigurations.OrganizationConfiguration>
{
    public void Configure(EntityTypeBuilder<Domain.OrganizationConfigurations.OrganizationConfiguration> builder)
    {
        builder.ToTable("organization_configurations");
        builder.HasKey(configuration => configuration.Id);

        builder.Property(configuration => configuration.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new OrganizationConfigurationId(value))
            .ValueGeneratedNever();

        builder.Property(configuration => configuration.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(configuration => configuration.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(configuration => configuration.VersionNumber)
            .HasColumnName("version_number")
            .IsRequired();

        builder.Property(configuration => configuration.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        builder.OwnsOne(configuration => configuration.Payload, payload =>
        {
            payload.Property(value => value.Json)
                .HasColumnName("settings_json")
                .HasColumnType("jsonb")
                .IsRequired();
        });

        builder.Property(configuration => configuration.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(configuration => configuration.EffectiveFromUtc)
            .HasColumnName("effective_from_utc");
        builder.Property(configuration => configuration.EffectiveToUtc)
            .HasColumnName("effective_to_utc");
        builder.Property(configuration => configuration.PublishedAtUtc)
            .HasColumnName("published_at_utc");
        builder.Property(configuration => configuration.PublishedByUserId)
            .HasColumnName("published_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.Property(configuration => configuration.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(configuration => configuration.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();
        builder.Property(configuration => configuration.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(configuration => configuration.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");
        builder.Property(configuration => configuration.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.HasIndex(configuration => new { configuration.OrganizationId, configuration.VersionNumber })
            .IsUnique()
            .HasDatabaseName("ux_organization_configurations_organization_version");

        builder.HasIndex(configuration => new { configuration.OrganizationId, configuration.Status, configuration.EffectiveFromUtc })
            .HasDatabaseName("ix_organization_configurations_effective_lookup");

        builder.Ignore(configuration => configuration.DomainEvents);
    }
}
