using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class GuardianRelationshipConfiguration
    : IEntityTypeConfiguration<GuardianRelationship>
{
    public void Configure(EntityTypeBuilder<GuardianRelationship> b)
    {
        b.ToTable("guardian_relationships");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new GuardianRelationshipId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new(x))
            .HasColumnName("organization_id");
        b.Property(x => x.StudentId)
            .HasConversion(x => x.Value, x => new(x))
            .HasColumnName("student_id");
        b.Property(x => x.GuardianPersonId)
            .HasConversion(x => x.Value, x => new(x))
            .HasColumnName("guardian_person_id");
        b.Property(x => x.GuardianFirstName).HasMaxLength(100).HasColumnName("guardian_first_name");
        b.Property(x => x.GuardianLastName).HasMaxLength(100).HasColumnName("guardian_last_name");
        b.Property(x => x.GuardianEmail).HasMaxLength(254).HasColumnName("guardian_email");
        b.Property(x => x.GuardianPhone).HasMaxLength(40).HasColumnName("guardian_phone");
        b.Property(x => x.RelationshipType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("relationship_type");
        b.Property(x => x.LegalBasis).HasMaxLength(500).HasColumnName("legal_basis");
        b.Property(x => x.ParentalAuthorityStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasColumnName("parental_authority_status");
        b.Property(x => x.Permissions).HasConversion<long>().HasColumnName("permissions");
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.FinancialRights).HasColumnName("financial_rights");
        b.Property(x => x.SignatureRights).HasColumnName("signature_rights");
        b.Property(x => x.NotificationPreferences)
            .HasMaxLength(500)
            .HasColumnName("notification_preferences");
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).HasColumnName("status");
        b.Property(x => x.InvitedAtUtc).HasColumnName("invited_at_utc");
        b.Property(x => x.InvitedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("invited_by_user_id");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId)
            .HasConversion(x => x.Value, x => new UserId(x))
            .HasColumnName("created_by_user_id");
        b.Property(x => x.ModifiedAtUtc).HasColumnName("modified_at_utc");
        b.Property(x => x.ModifiedByUserId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            )
            .HasColumnName("modified_by_user_id");
        b.Property(x => x.RevocationReason).HasMaxLength(500).HasColumnName("revocation_reason");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.Status,
            })
            .HasDatabaseName("ix_guardian_relationships_owner_student_status");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.GuardianPersonId,
            })
            .HasDatabaseName("ix_guardian_relationships_owner_student_guardian");
        b.HasIndex(x => x.GuardianPersonId);
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
