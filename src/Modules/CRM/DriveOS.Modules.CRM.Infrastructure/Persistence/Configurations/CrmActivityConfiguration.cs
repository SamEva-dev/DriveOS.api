using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class CrmActivityConfiguration : IEntityTypeConfiguration<CrmActivity>
{
    public void Configure(EntityTypeBuilder<CrmActivity> b)
    {
        b.ToTable("activities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id")
            .HasConversion(x => x.Value, x => new CrmActivityId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.LeadId).HasColumnName("lead_id")
            .HasConversion(x => x.Value, x => new LeadId(x));
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Direction).HasColumnName("direction").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(200).IsRequired();
        b.Property(x => x.Details).HasColumnName("details").HasMaxLength(4000);
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.LeadId, x.OccurredAtUtc })
            .HasDatabaseName("ix_activities_organization_lead_occurred");
        b.HasIndex(x => new { x.OrganizationId, x.OccurredAtUtc })
            .HasDatabaseName("ix_activities_organization_occurred");
        b.Ignore(x => x.DomainEvents);
    }
}
