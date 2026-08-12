using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class SavedLeadViewConfiguration : IEntityTypeConfiguration<SavedLeadView>
{
    public void Configure(EntityTypeBuilder<SavedLeadView> b)
    {
        b.ToTable("saved_lead_views"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.OwnerUserId).HasColumnName("owner_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        b.Property(x => x.FiltersJson).HasColumnName("filters_json").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.SortJson).HasColumnName("sort_json").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.ColumnsJson).HasColumnName("columns_json").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Scope).HasColumnName("scope").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.BranchId).HasColumnName("branch_id");
        b.Property(x => x.IsDefault).HasColumnName("is_default");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.HasIndex(x => new { x.OrganizationId, x.OwnerUserId, x.Name }).IsUnique()
            .HasDatabaseName("ux_saved_lead_views_owner_name");
        b.HasIndex(x => new { x.OrganizationId, x.Scope, x.BranchId })
            .HasDatabaseName("ix_saved_lead_views_scope");
    }
}
