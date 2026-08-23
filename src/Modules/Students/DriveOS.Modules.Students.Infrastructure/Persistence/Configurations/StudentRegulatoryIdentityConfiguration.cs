using DriveOS.Modules.Students.Domain.RegulatoryIdentities;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentRegulatoryIdentityConfiguration : IEntityTypeConfiguration<StudentRegulatoryIdentity>
{
    public void Configure(EntityTypeBuilder<StudentRegulatoryIdentity> b)
    {
        b.ToTable("student_regulatory_identities");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new StudentRegulatoryIdentityId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        b.Property(x => x.IdentifierType).HasColumnName("identifier_type").HasMaxLength(40).IsRequired();
        b.Property(x => x.Value).HasColumnName("identifier_value").HasMaxLength(100).IsRequired();
        b.Property(x => x.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.DeclaredAtUtc).HasColumnName("declared_at_utc");
        b.Property(x => x.DeclaredByUserId)
            .HasColumnName("declared_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.VerifiedAtUtc).HasColumnName("verified_at_utc");
        b.Property(x => x.VerifiedByUserId)
            .HasColumnName("verified_by_user_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.VerificationMethod).HasColumnName("verification_method").HasMaxLength(80);
        b.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        b.Property(x => x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        b.Property(x => x.SupersededById)
            .HasColumnName("superseded_by_id")
            .HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new StudentRegulatoryIdentityId(x.Value) : null);

        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.CountryCode, x.IdentifierType })
            .HasDatabaseName("ix_student_regulatory_identities_lookup");
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.CountryCode, x.IdentifierType })
            .IsUnique()
            .HasFilter("status IN ('Declared', 'Verified')")
            .HasDatabaseName("ux_student_regulatory_identities_current");
        b.Ignore(x => x.IsCurrent);
        b.Ignore(x => x.DomainEvents);
    }
}
