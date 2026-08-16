using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class NetworkOrganizationMembershipConfiguration
    : IEntityTypeConfiguration<NetworkOrganizationMembership>
{
    public void Configure(EntityTypeBuilder<NetworkOrganizationMembership> builder)
    {
        builder.ToTable("network_organization_memberships");
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .HasConversion(x => x.Value, x => new NetworkOrganizationMembershipId(x));
        builder
            .Property(x => x.NetworkOrganizationId)
            .HasColumnName("network_organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        builder
            .Property(x => x.MemberOrganizationId)
            .HasColumnName("member_organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.JoinedAtUtc).HasColumnName("joined_at_utc");
        builder.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Ignore(x => x.IsActive);
        builder
            .HasIndex(x => new { x.NetworkOrganizationId, x.MemberOrganizationId })
            .IsUnique()
            .HasDatabaseName("ux_network_memberships_network_member");
        builder
            .HasIndex(x => x.MemberOrganizationId)
            .IsUnique()
            .HasFilter("ended_at_utc IS NULL")
            .HasDatabaseName("ux_network_memberships_active_member");
        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.NetworkOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.MemberOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
