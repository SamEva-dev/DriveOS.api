using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FleetResources.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {
        b.ToTable("vehicles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new VehicleId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.OwnerOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.ProviderOrganizationId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new OrganizationId(x.Value));
        b.Property(x => x.BranchId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new BranchId(x.Value));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.RegistrationNumber).HasMaxLength(32).IsRequired();
        b.Property(x => x.Vin).HasMaxLength(64);
        b.Property(x => x.Make).HasMaxLength(120);
        b.Property(x => x.Model).HasMaxLength(120);
        b.Property(x => x.TransmissionType).HasMaxLength(64).IsRequired();
        b.Property(x => x.EnergyType).HasMaxLength(64).IsRequired();
        b.Property(x => x.LicenseCategoriesCsv).HasMaxLength(512).IsRequired();
        b.Property(x => x.AdaptationsCsv).HasMaxLength(1024).IsRequired();
        b.Property(x => x.ComplianceNotes).HasMaxLength(2000);
        b.Property(x => x.CurrentOdometerKilometers).HasDefaultValue(0L);
        b.HasIndex(x => new { x.OrganizationId, x.RegistrationNumber }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.BranchId, x.OperationalStatus });
    }
}
