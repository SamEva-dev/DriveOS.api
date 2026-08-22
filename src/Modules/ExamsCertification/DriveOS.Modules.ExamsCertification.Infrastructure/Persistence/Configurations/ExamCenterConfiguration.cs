using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamCenterConfiguration : IEntityTypeConfiguration<ExamCenter>
{
    public void Configure(EntityTypeBuilder<ExamCenter> builder)
    {
        builder.ToTable("exam_centers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamCenterId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.AdministrativeAreaCode).HasColumnName("administrative_area_code").HasMaxLength(64);
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(1000);
        builder.Property(x => x.ExternalProviderCode).HasColumnName("external_provider_code").HasMaxLength(100);
        builder.Property(x => x.ExternalCenterId).HasColumnName("external_center_id").HasMaxLength(200);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.CountryCode, x.Name }).HasDatabaseName("ix_exam_center_org_country_name");
        builder.HasIndex(x => new { x.OrganizationId, x.ExternalProviderCode, x.ExternalCenterId }).IsUnique().HasFilter("external_provider_code IS NOT NULL AND external_center_id IS NOT NULL").HasDatabaseName("ux_exam_center_external");
        builder.Ignore(x => x.DomainEvents);
    }
}
