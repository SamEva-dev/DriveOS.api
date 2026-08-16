using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.ToTable("students");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new PersonId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(254);
        b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(40);
        b.Property(x => x.PreferredName).HasColumnName("preferred_name").HasMaxLength(100);
        b.Property(x => x.BirthDate).HasColumnName("birth_date");
        b.Property(x => x.BirthPlace).HasColumnName("birth_place").HasMaxLength(150);
        b.Property(x => x.Nationality).HasColumnName("nationality").HasMaxLength(80);
        b.Property(x => x.AddressLine1).HasColumnName("address_line1").HasMaxLength(200);
        b.Property(x => x.AddressLine2).HasColumnName("address_line2").HasMaxLength(200);
        b.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20);
        b.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
        b.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(3);
        b.Property(x => x.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(10);
        b.Property(x => x.TimeZone).HasColumnName("time_zone").HasMaxLength(100);
        b.Property(x => x.AllowEmail).HasColumnName("allow_email");
        b.Property(x => x.AllowSms).HasColumnName("allow_sms");
        b.Property(x => x.AllowPhone).HasColumnName("allow_phone");
        b.Property(x => x.IdentityVerificationStatus)
            .HasColumnName("identity_verification_status")
            .HasConversion<string>()
            .HasMaxLength(30);
        b.Property(x => x.IdentityVerifiedAtUtc).HasColumnName("identity_verified_at_utc");
        b.Property(x => x.IdentityVerifiedByUserId)
            .HasColumnName("identity_verified_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasIndex(x => new { x.OrganizationId, x.Status })
            .HasDatabaseName("ix_students_organization_status");
        b.HasMany(x => x.IdentityAuditEntries)
            .WithOne()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
