using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamProviderConnectionConfiguration : IEntityTypeConfiguration<ExamProviderConnection>
{
    public void Configure(EntityTypeBuilder<ExamProviderConnection> builder)
    {
        builder.ToTable("exam_provider_connections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamProviderConnectionId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.AuthenticationMode).HasColumnName("authentication_mode").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.BaseUrl).HasColumnName("base_url").HasMaxLength(1000);
        builder.Property(x => x.CredentialReference).HasColumnName("credential_reference").HasMaxLength(1000);
        builder.Property(x => x.RequestsPerMinute).HasColumnName("requests_per_minute");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.LastTestedAtUtc).HasColumnName("last_tested_at_utc");
        builder.Property(x => x.LastSuccessfulAtUtc).HasColumnName("last_successful_at_utc");
        builder.Property(x => x.LastErrorCode).HasColumnName("last_error_code").HasMaxLength(200);
        builder.Property(x => x.ConsecutiveFailureCount).HasColumnName("consecutive_failure_count");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.ProviderCode }).IsUnique().HasDatabaseName("ux_exam_provider_connection_tenant_provider");
        builder.Ignore(x => x.DomainEvents);
    }
}
