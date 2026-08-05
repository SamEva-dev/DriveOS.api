using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

public sealed class OrganizationClosureConfiguration : IEntityTypeConfiguration<OrganizationClosure>
{
    public void Configure(EntityTypeBuilder<OrganizationClosure> builder)
    {
        builder.ToTable("organization_closures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id")
            .HasConversion(id => id.Value, value => new OrganizationClosureId(value));
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value)).IsRequired();
        builder.Property(x => x.ReasonCode).HasColumnName("reason_code").HasConversion<int>().IsRequired();
        builder.Property(x => x.ReasonDetails).HasColumnName("reason_details").HasMaxLength(OrganizationClosure.MaximumDetailsLength);
        builder.Property(x => x.RequestedEffectiveAtUtc).HasColumnName("requested_effective_at_utc").IsRequired();
        builder.Property(x => x.DataDisposition).HasColumnName("data_disposition").HasConversion<int>().IsRequired();
        builder.Property(x => x.RetentionUntilUtc).HasColumnName("retention_until_utc");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.RequestedByUserId).HasColumnName("requested_by_user_id")
            .HasConversion(id => id.Value, value => new UserId(value)).IsRequired();
        builder.Property(x => x.ReviewedByUserId).HasColumnName("reviewed_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(x => x.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        builder.Property(x => x.ScheduledAtUtc).HasColumnName("scheduled_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        builder.Property(x => x.DecisionComment).HasColumnName("decision_comment").HasMaxLength(OrganizationClosure.MaximumDetailsLength);
        builder.Property(x => x.Revision).HasColumnName("revision").IsConcurrencyToken().IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? new UserId(value.Value) : null);

        builder.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.OrganizationId, x.Status })
            .HasDatabaseName("ix_organization_closures_org_status");
        builder.HasIndex(x => x.OrganizationId).IsUnique()
            .HasFilter("status IN (1, 2, 3, 4)")
            .HasDatabaseName("ux_organization_closures_open_per_org");
        builder.Ignore(x => x.IsOpen);
        builder.Ignore(x => x.DomainEvents);
    }
}
