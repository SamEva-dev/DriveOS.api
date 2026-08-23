using DriveOS.Modules.Organizations.Domain.InstructorRegulatoryCredentials;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class InstructorRegulatoryCredentialConfiguration : IEntityTypeConfiguration<InstructorRegulatoryCredential>
{
    public void Configure(EntityTypeBuilder<InstructorRegulatoryCredential> b)
    {
        b.ToTable("instructor_regulatory_credentials", OrganizationsSchema.Name);
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new InstructorRegulatoryCredentialId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.InstructorUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        b.Property(x => x.CredentialType).HasMaxLength(64).IsRequired();
        b.Property(x => x.Identifier).HasMaxLength(120).IsRequired();
        b.Property(x => x.IssuingAuthority).HasMaxLength(160).IsRequired();
        b.Property(x => x.JurisdictionCode).HasMaxLength(40);
        b.Property(x => x.Source).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.DeclaredByUserId).HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.VerifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.VerificationMethod).HasMaxLength(80);
        b.Property(x => x.DecisionReason).HasMaxLength(500);
        b.Property(x => x.SupersededById).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new InstructorRegulatoryCredentialId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.InstructorUserId });
        b.HasIndex(x => new { x.OrganizationId, x.InstructorUserId, x.CountryCode, x.CredentialType })
            .HasFilter("\"Status\" IN (0, 1)").IsUnique().HasDatabaseName("ux_instructor_regulatory_credentials_current");
        b.Ignore(x => x.DomainEvents);
    }
}
