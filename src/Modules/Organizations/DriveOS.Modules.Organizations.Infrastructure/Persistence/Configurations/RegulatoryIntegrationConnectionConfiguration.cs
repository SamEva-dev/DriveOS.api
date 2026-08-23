using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class RegulatoryIntegrationConnectionConfiguration : IEntityTypeConfiguration<RegulatoryIntegrationConnection>
{
    public void Configure(EntityTypeBuilder<RegulatoryIntegrationConnection> builder)
    {
        builder.ToTable("regulatory_integration_connections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(id => id.Value, value => new RegulatoryIntegrationConnectionId(value)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(id => id.Value, value => new OrganizationId(value)).IsRequired();
        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.BranchId).HasColumnName("branch_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new BranchId(value.Value) : null);
        builder.Property(x => x.ScopeKey).HasColumnName("scope_key").HasMaxLength(80).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalAccountReference).HasColumnName("external_account_reference").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretReference).HasColumnName("secret_reference").HasMaxLength(300);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Revision).HasColumnName("revision").IsConcurrencyToken().IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.ScopeKey, x.CountryCode, x.ProviderCode }).IsUnique().HasDatabaseName("ux_regulatory_integration_connection_scope_provider");
        builder.HasIndex(x => new { x.OrganizationId, x.CountryCode, x.ProviderCode, x.Status }).HasDatabaseName("ix_regulatory_integration_connection_resolve");
        builder.Ignore(x => x.DomainEvents);
    }
}
