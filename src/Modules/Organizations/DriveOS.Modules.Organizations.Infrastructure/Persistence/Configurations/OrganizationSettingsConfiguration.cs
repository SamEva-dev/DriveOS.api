using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationSettingsConfiguration
    : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("organization_settings");

        builder.HasKey(settings => settings.Id);

        builder
            .Property(settings => settings.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new OrganizationSettingsId(value))
            .ValueGeneratedNever();

        builder
            .Property(settings => settings.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .HasOne<Organization>()
            .WithOne()
            .HasForeignKey<OrganizationSettings>(settings => settings.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(
            settings => settings.Profile,
            profile =>
            {
                profile
                    .Property(value => value.TradeName)
                    .HasColumnName("trade_name")
                    .HasMaxLength(OrganizationProfile.TradeNameMaximumLength);

                profile
                    .Property(value => value.RegistrationNumber)
                    .HasColumnName("registration_number")
                    .HasMaxLength(OrganizationProfile.RegistrationNumberMaximumLength);

                profile
                    .Property(value => value.TaxNumber)
                    .HasColumnName("tax_number")
                    .HasMaxLength(OrganizationProfile.TaxNumberMaximumLength);
            }
        );

        builder.OwnsOne(
            settings => settings.Contact,
            contact =>
            {
                contact
                    .Property(value => value.Email)
                    .HasColumnName("contact_email")
                    .HasMaxLength(OrganizationContactInformation.EmailMaximumLength);

                contact
                    .Property(value => value.Phone)
                    .HasColumnName("contact_phone")
                    .HasMaxLength(OrganizationContactInformation.PhoneMaximumLength);

                contact
                    .Property(value => value.Website)
                    .HasColumnName("website")
                    .HasMaxLength(OrganizationContactInformation.WebsiteMaximumLength);
            }
        );

        builder.OwnsOne(
            settings => settings.Address,
            address =>
            {
                address
                    .Property(value => value.Line1)
                    .HasColumnName("address_line1")
                    .HasMaxLength(OrganizationAddress.AddressLineMaximumLength);

                address
                    .Property(value => value.Line2)
                    .HasColumnName("address_line2")
                    .HasMaxLength(OrganizationAddress.AddressLineMaximumLength);

                address
                    .Property(value => value.PostalCode)
                    .HasColumnName("postal_code")
                    .HasMaxLength(OrganizationAddress.PostalCodeMaximumLength);

                address
                    .Property(value => value.City)
                    .HasColumnName("city")
                    .HasMaxLength(OrganizationAddress.CityMaximumLength);

                address
                    .Property(value => value.Region)
                    .HasColumnName("region")
                    .HasMaxLength(OrganizationAddress.RegionMaximumLength);

                address
                    .Property(value => value.CountryCode)
                    .HasColumnName("address_country_code")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            settings => settings.Regional,
            regional =>
            {
                regional
                    .Property(value => value.DefaultLanguage)
                    .HasColumnName("default_language")
                    .HasMaxLength(15)
                    .IsRequired();

                regional
                    .Property(value => value.SupportedLanguages)
                    .HasColumnName("supported_languages")
                    .HasMaxLength(500)
                    .IsRequired();

                regional.Ignore(value => value.SupportedLanguageCodes);

                regional
                    .Property(value => value.TimeZoneId)
                    .HasColumnName("time_zone_id")
                    .HasMaxLength(OrganizationRegionalSettings.TimeZoneMaximumLength)
                    .IsRequired();

                regional
                    .Property(value => value.CurrencyCode)
                    .HasColumnName("currency_code")
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .IsRequired();

                regional
                    .Property(value => value.DateFormat)
                    .HasColumnName("date_format")
                    .HasMaxLength(OrganizationRegionalSettings.FormatMaximumLength)
                    .IsRequired();

                regional
                    .Property(value => value.TimeFormat)
                    .HasColumnName("time_format")
                    .HasMaxLength(OrganizationRegionalSettings.FormatMaximumLength)
                    .IsRequired();

                regional
                    .Property(value => value.FirstDayOfWeek)
                    .HasColumnName("first_day_of_week")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                regional
                    .Property(value => value.MeasurementSystem)
                    .HasColumnName("measurement_system")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            settings => settings.Operational,
            operational =>
            {
                operational
                    .Property(value => value.DefaultSessionDurationMinutes)
                    .HasColumnName("default_session_duration_minutes")
                    .IsRequired();

                operational
                    .Property(value => value.DefaultBookingLeadTimeMinutes)
                    .HasColumnName("default_booking_lead_time_minutes")
                    .IsRequired();

                operational
                    .Property(value => value.DefaultCancellationDelayHours)
                    .HasColumnName("default_cancellation_delay_hours")
                    .IsRequired();

                operational
                    .Property(value => value.AllowStudentSelfBooking)
                    .HasColumnName("allow_student_self_booking")
                    .IsRequired();

                operational
                    .Property(value => value.RequireBranchForOperations)
                    .HasColumnName("require_branch_for_operations")
                    .IsRequired();

                operational
                    .Property(value => value.DefaultBranchId)
                    .HasColumnName("default_branch_id")
                    .HasConversion(
                        id => id.HasValue ? id.Value.Value : (Guid?)null,
                        value => value.HasValue ? new BranchId(value.Value) : null
                    );
            }
        );

        builder
            .Property(settings => settings.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(settings => settings.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(settings => settings.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(settings => settings.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder
            .Property(settings => settings.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .HasIndex(settings => settings.OrganizationId)
            .IsUnique()
            .HasDatabaseName("ux_organization_settings_organization_id");

        builder.Ignore(settings => settings.DomainEvents);
    }
}
