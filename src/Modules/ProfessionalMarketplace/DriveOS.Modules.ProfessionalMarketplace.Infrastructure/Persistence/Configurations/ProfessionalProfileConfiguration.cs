using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalProfileConfiguration : IEntityTypeConfiguration<ProfessionalProfile>
{
    public void Configure(EntityTypeBuilder<ProfessionalProfile> b)
    {
        b.ToTable("professional_profiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ProfessionalProfileId(x)).ValueGeneratedNever();
        b.Property(x => x.PersonId).HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.ProviderOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.UserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.ComplianceStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.ComplianceEvaluatedAtUtc);
        b.Property(x => x.MarketplaceVisibility).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.VerificationBadge).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.ComplianceEnforcementAction).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.ComplianceEnforcementReason).HasMaxLength(1000);
        b.Ignore(x => x.IsDiscoverable);
        b.Property(x => x.ProfessionalType).HasConversion<string>().HasMaxLength(48).IsRequired();
        b.Property(x => x.LegalName).HasMaxLength(180); b.Property(x => x.TradeName).HasMaxLength(180); b.Property(x => x.LegalStatusCode).HasMaxLength(64); b.Property(x => x.RegistrationNumber).HasMaxLength(80); b.Property(x => x.TaxNumber).HasMaxLength(80);
        b.Property(x => x.ProfessionalEmail).HasMaxLength(254); b.Property(x => x.ProfessionalPhone).HasMaxLength(48);
        b.Property(x => x.BillingAddressLine1).HasMaxLength(200); b.Property(x => x.BillingAddressLine2).HasMaxLength(200); b.Property(x => x.BillingPostalCode).HasMaxLength(32); b.Property(x => x.BillingCity).HasMaxLength(120); b.Property(x => x.BillingCountryCode).HasMaxLength(2);
        b.Property(x => x.Headline).HasMaxLength(160); b.Property(x => x.Biography).HasMaxLength(2000);
        b.Property(x => x.Languages).HasColumnType("text[]").IsRequired(); b.Property(x => x.TeachingCategoryCodes).HasColumnType("text[]").IsRequired(); b.Property(x => x.SpecializationCodes).HasColumnType("text[]").IsRequired(); b.Property(x => x.PreferredEngagementTypes).HasColumnType("text[]").IsRequired();
        var capabilitiesComparer = new ValueComparer<TeachingCapability[]>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<TeachingCapability[]>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? Array.Empty<TeachingCapability>());
        b.Property(x => x.TeachingCapabilities)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<TeachingCapability[]>(value, (JsonSerializerOptions?)null) ?? Array.Empty<TeachingCapability>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(capabilitiesComparer);




        var ratesComparer = new ValueComparer<ProfessionalRate[]>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<ProfessionalRate[]>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? Array.Empty<ProfessionalRate>());
        b.Property(x => x.Rates)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<ProfessionalRate[]>(value, (JsonSerializerOptions?)null) ?? Array.Empty<ProfessionalRate>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(ratesComparer);

        var availabilityComparer = new ValueComparer<MarketplaceAvailabilityPolicy>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<MarketplaceAvailabilityPolicy>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)
                     ?? new MarketplaceAvailabilityPolicy(Array.Empty<MarketplaceAvailabilityRule>(), Array.Empty<MarketplaceAvailabilityException>(), 24, 600, 300));
        b.Property(x => x.AvailabilityPolicy)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<MarketplaceAvailabilityPolicy>(value, (JsonSerializerOptions?)null)
                         ?? new MarketplaceAvailabilityPolicy(Array.Empty<MarketplaceAvailabilityRule>(), Array.Empty<MarketplaceAvailabilityException>(), 24, 600, 300))
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(availabilityComparer);

        var serviceAreasComparer = new ValueComparer<ProfessionalServiceArea[]>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<ProfessionalServiceArea[]>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? Array.Empty<ProfessionalServiceArea>());
        b.Property(x => x.ServiceAreas)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<ProfessionalServiceArea[]>(value, (JsonSerializerOptions?)null) ?? Array.Empty<ProfessionalServiceArea>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(serviceAreasComparer);

        b.Property(x => x.PrimaryServiceArea).HasMaxLength(160); b.Property(x => x.PersonalVehicleNotes).HasMaxLength(500);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Ignore(x => x.IsProfileComplete);

        b.HasIndex(x => x.PersonId).IsUnique();
        b.HasIndex(x => x.ProviderOrganizationId);
        b.HasIndex(x => x.RegistrationNumber);
        b.HasIndex(x => new { x.Status, x.ComplianceStatus });
        b.HasIndex(x => new { x.BillingCountryCode, x.ProfessionalType, x.Status });
    }
}
