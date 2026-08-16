using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");

        builder.HasKey(branch => branch.Id);

        builder
            .Property(branch => branch.Id)
            .HasConversion(id => id.Value, value => new BranchId(value))
            .ValueGeneratedNever();

        builder
            .Property(branch => branch.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .Property(branch => branch.Name)
            .HasColumnName("name")
            .HasConversion(name => name.Value, value => BranchName.Create(value).Value)
            .HasMaxLength(BranchName.MaximumLength)
            .IsRequired();

        builder
            .Property(branch => branch.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(BranchName.MaximumLength)
            .IsRequired();

        builder
            .Property(branch => branch.Code)
            .HasColumnName("code")
            .HasConversion(code => code.Value, value => BranchCode.Create(value).Value)
            .HasMaxLength(BranchCode.MaximumLength)
            .IsRequired();

        builder
            .Property(branch => branch.Type)
            .HasColumnName("branch_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(branch => branch.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(
            branch => branch.Address,
            address =>
            {
                address
                    .Property(value => value.Line1)
                    .HasColumnName("address_line1")
                    .HasMaxLength(BranchAddress.AddressLineMaximumLength)
                    .IsRequired();

                address
                    .Property(value => value.Line2)
                    .HasColumnName("address_line2")
                    .HasMaxLength(BranchAddress.AddressLineMaximumLength);

                address
                    .Property(value => value.PostalCode)
                    .HasColumnName("postal_code")
                    .HasMaxLength(BranchAddress.PostalCodeMaximumLength)
                    .IsRequired();

                address
                    .Property(value => value.City)
                    .HasColumnName("city")
                    .HasMaxLength(BranchAddress.CityMaximumLength)
                    .IsRequired();

                address
                    .Property(value => value.CountryCode)
                    .HasColumnName("country_code")
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .IsRequired();
            }
        );

        builder
            .Property(branch => branch.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(branch => branch.IsPrimary).HasColumnName("is_primary").IsRequired();

        builder
            .Property(branch => branch.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(branch => branch.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder.Property(branch => branch.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");

        builder
            .Property(branch => branch.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .HasMany(branch => branch.StatusHistory)
            .WithOne()
            .HasForeignKey(entry => entry.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(branch => branch.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(branch => branch.ManagerAssignments)
            .WithOne()
            .HasForeignKey(assignment => assignment.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(branch => new { branch.OrganizationId, branch.NormalizedName })
            .IsUnique()
            .HasDatabaseName("ux_branches_organization_normalized_name");

        builder
            .HasIndex(branch => new { branch.OrganizationId, branch.Code })
            .IsUnique()
            .HasDatabaseName("ux_branches_organization_code");

        builder
            .HasIndex(branch => new { branch.OrganizationId, branch.Status })
            .HasDatabaseName("ix_branches_organization_status");

        builder
            .HasIndex(branch => new { branch.OrganizationId, branch.IsPrimary })
            .IsUnique()
            .HasFilter("is_primary = true AND status <> 'Closed'")
            .HasDatabaseName("ux_branches_primary_per_organization");

        builder.Ignore(branch => branch.DomainEvents);
    }
}
