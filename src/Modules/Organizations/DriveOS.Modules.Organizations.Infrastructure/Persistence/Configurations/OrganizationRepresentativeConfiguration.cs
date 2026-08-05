using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

public sealed class OrganizationRepresentativeConfiguration : IEntityTypeConfiguration<OrganizationRepresentative>
{
    public void Configure(EntityTypeBuilder<OrganizationRepresentative> builder)
    {
        builder.ToTable("organization_representatives");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(id => id.Value, value => new OrganizationRepresentativeId(value));
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(id => id.Value, value => new OrganizationId(value)).IsRequired();
        builder.Property(x => x.PersonId).HasColumnName("person_id").HasConversion(id => id.Value, value => new PersonId(value)).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(x => x.RepresentativeType).HasColumnName("representative_type").HasConversion<int>().IsRequired();
        builder.OwnsOne(x => x.AuthorityScope, scope =>
        {
            scope.Property(x => x.Value).HasColumnName("authority_scope").HasMaxLength(RepresentativeAuthorityScope.MaximumLength).IsRequired();
        });
        builder.Property(x => x.IsPrimaryOwner).HasColumnName("is_primary_owner").IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.Revision).HasColumnName("revision").IsConcurrencyToken().IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);

        builder.HasOne<DriveOS.Modules.Organizations.Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.PersonId, x.RepresentativeType })
            .HasDatabaseName("ix_organization_representatives_org_person_type");

        builder.HasIndex(x => new { x.OrganizationId, x.Status })
            .HasDatabaseName("ix_organization_representatives_org_status");

        builder.HasIndex(x => x.OrganizationId)
            .IsUnique()
            .HasFilter("is_primary_owner = TRUE AND status = 2")
            .HasDatabaseName("ux_organization_representatives_primary_owner");

        builder.HasIndex(x => new { x.OrganizationId, x.PersonId, x.RepresentativeType })
            .IsUnique()
            .HasFilter("status IN (1, 2, 3)")
            .HasDatabaseName("ux_organization_representatives_active_identity");

        builder.Ignore(x => x.DomainEvents);
    }
}
