using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalMissionConfiguration : IEntityTypeConfiguration<ProfessionalMission>
{
    public void Configure(EntityTypeBuilder<ProfessionalMission> b)
    {
        b.ToTable("professional_missions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new ProfessionalMissionId(x))
            .ValueGeneratedNever();

        b.Property(x => x.EngagementId)
            .HasConversion(x => x.Value, x => new ProfessionalEngagementId(x))
            .IsRequired();

        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .IsRequired();

        b.Property(x => x.ProfessionalProfileId)
            .HasConversion(x => x.Value, x => new ProfessionalProfileId(x))
            .IsRequired();

        b.Property(x => x.BranchId)
            .HasConversion(
                x => x == null ? (Guid?)null : x.Value.Value,
                x => x == null ? null : new BranchId(x.Value));

        b.Property(x => x.Title).HasMaxLength(180).IsRequired();
        b.Property(x => x.Description).HasMaxLength(3000);
        b.Property(x => x.TeachingCategoryCodes).HasColumnType("text[]").IsRequired();
        b.Property(x => x.VehicleProvisionMode).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        b.Property(x => x.StatusReason).HasMaxLength(512);

        var windowsComparer = new ValueComparer<MissionTimeWindow[]>(
            (left, right) =>
                JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) ==
                JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<MissionTimeWindow[]>(
                         JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                         (JsonSerializerOptions?)null) ?? Array.Empty<MissionTimeWindow>());

        b.Property(x => x.TimeWindows)
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<MissionTimeWindow[]>(
                             value,
                             (JsonSerializerOptions?)null) ?? Array.Empty<MissionTimeWindow>())
            .HasColumnType("jsonb")
            .Metadata.SetValueComparer(windowsComparer);

        b.HasIndex(x => new { x.EngagementId, x.Status });
        b.HasIndex(x => new { x.OrganizationId, x.Status });
        b.HasIndex(x => new { x.ProfessionalProfileId, x.Status });
        b.HasIndex(x => new { x.BranchId, x.StartsOn, x.EndsOn });

        b.Ignore(x => x.DomainEvents);
    }
}
