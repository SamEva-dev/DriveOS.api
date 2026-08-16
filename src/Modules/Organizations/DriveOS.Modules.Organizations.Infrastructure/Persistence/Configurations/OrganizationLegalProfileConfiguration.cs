using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationLegalProfileConfiguration
    : IEntityTypeConfiguration<OrganizationLegalProfile>
{
    public void Configure(EntityTypeBuilder<OrganizationLegalProfile> builder)
    {
        builder.ToTable("organization_legal_profiles");
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new OrganizationLegalProfileId(value))
            .ValueGeneratedNever();

        builder
            .Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .Property(x => x.LegalForm)
            .HasColumnName("legal_form")
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder
            .Property(x => x.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.TaxNumber).HasColumnName("tax_number").HasMaxLength(80);
        builder.Property(x => x.TradeName).HasColumnName("trade_name").HasMaxLength(200);
        builder.Property(x => x.IncorporationDate).HasColumnName("incorporation_date");
        builder
            .Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder
            .Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken()
            .IsRequired();

        builder.OwnsOne(
            x => x.RegisteredAddress,
            address =>
            {
                address
                    .Property(x => x.Line1)
                    .HasColumnName("registered_address_line1")
                    .HasMaxLength(200)
                    .IsRequired();
                address
                    .Property(x => x.Line2)
                    .HasColumnName("registered_address_line2")
                    .HasMaxLength(200);
                address
                    .Property(x => x.PostalCode)
                    .HasColumnName("registered_postal_code")
                    .HasMaxLength(30)
                    .IsRequired();
                address
                    .Property(x => x.City)
                    .HasColumnName("registered_city")
                    .HasMaxLength(120)
                    .IsRequired();
                address
                    .Property(x => x.Region)
                    .HasColumnName("registered_region")
                    .HasMaxLength(120);
                address
                    .Property(x => x.CountryCode)
                    .HasColumnName("registered_country_code")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();
            }
        );

        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder
            .Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v.HasValue ? new UserId(v.Value) : null
            );
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder
            .Property(x => x.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                v => v.HasValue ? v.Value.Value : (Guid?)null,
                v => v.HasValue ? new UserId(v.Value) : null
            );

        builder
            .HasIndex(x => x.OrganizationId)
            .IsUnique()
            .HasDatabaseName("ux_organization_legal_profiles_organization");

        builder
            .HasIndex(x => new { x.RegistrationNumber, x.Status })
            .HasDatabaseName("ix_organization_legal_profiles_registration_status");

        builder.Ignore(x => x.DomainEvents);
    }
}
