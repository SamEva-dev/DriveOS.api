using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(lead => lead.Id);

        builder.Property(lead => lead.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new LeadId(value))
            .ValueGeneratedNever();

        builder.Property(lead => lead.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder.Property(lead => lead.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new BranchId(value.Value) : null);

        builder.Property(lead => lead.AssignedAdvisorId)
            .HasColumnName("assigned_advisor_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.Property(lead => lead.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(lead => lead.Identity, identity =>
        {
            identity.Property(value => value.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            identity.Property(value => value.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            identity.Property(value => value.Email)
                .HasColumnName("email")
                .HasMaxLength(254);

            identity.Property(value => value.Phone)
                .HasColumnName("phone")
                .HasMaxLength(40);
        });

        builder.OwnsOne(lead => lead.RequestedTraining, training =>
        {
            training.Property(value => value.LicenseCategory)
                .HasColumnName("requested_license_category")
                .HasMaxLength(30)
                .IsRequired();

            training.Property(value => value.Transmission)
                .HasColumnName("preferred_transmission")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            training.Property(value => value.PreferredLocation)
                .HasColumnName("preferred_location")
                .HasMaxLength(200);
        });

        builder.OwnsOne(lead => lead.Source, source =>
        {
            source.Property(value => value.Type)
                .HasColumnName("source_type")
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            source.Property(value => value.Detail)
                .HasColumnName("source_detail")
                .HasMaxLength(250);
        });

        builder.Property(lead => lead.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(lead => lead.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.Property(lead => lead.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder.Property(lead => lead.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.HasIndex(lead => lead.OrganizationId)
            .HasDatabaseName("ix_leads_organization_id");

        builder.HasIndex(lead => new { lead.OrganizationId, lead.Status })
            .HasDatabaseName("ix_leads_organization_status");

        builder.HasIndex(lead => new { lead.OrganizationId, lead.BranchId })
            .HasDatabaseName("ix_leads_organization_branch");

        builder.Ignore(lead => lead.DomainEvents);
    }
}
