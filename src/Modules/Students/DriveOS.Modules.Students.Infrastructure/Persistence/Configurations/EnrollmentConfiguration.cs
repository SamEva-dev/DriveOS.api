using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> b)
    {
        b.ToTable("enrollments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new DraftEnrollmentId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.SourceLeadId)
            .HasColumnName("source_lead_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new LeadId(x.Value) : null
            );
        b.Property(x => x.TrainingCode)
            .HasColumnName("training_code")
            .HasMaxLength(100)
            .IsRequired();
        b.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        b.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
        b.Property(x => x.RegulatoryCountryCode)
            .HasColumnName("regulatory_country_code")
            .HasMaxLength(3);
        b.Property(x => x.PreferredLanguageCode)
            .HasColumnName("preferred_language_code")
            .HasMaxLength(10);
        b.Property(x => x.RequiredConsentsAccepted).HasColumnName("required_consents_accepted");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
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
        b.HasIndex(x => new { x.OrganizationId, x.StudentId })
            .HasDatabaseName("ix_enrollments_organization_student");
        b.HasIndex(x => new { x.OrganizationId, x.SourceLeadId })
            .IsUnique()
            .HasFilter("source_lead_id IS NOT NULL")
            .HasDatabaseName("ux_enrollments_organization_source_lead");
        b.HasIndex(x => new { x.OrganizationId, x.IdempotencyKey })
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL")
            .HasDatabaseName("ux_enrollments_organization_idempotency_key");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.BranchId,
                x.Status,
            })
            .HasDatabaseName("ix_enrollments_organization_branch_status");
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.DomainEvents);
    }
}
