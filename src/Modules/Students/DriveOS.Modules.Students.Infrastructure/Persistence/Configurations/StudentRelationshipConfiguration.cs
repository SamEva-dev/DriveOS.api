using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentRelationshipConfiguration
    : IEntityTypeConfiguration<StudentRelationship>
{
    public void Configure(EntityTypeBuilder<StudentRelationship> b)
    {
        b.ToTable("student_relationships");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new StudentRelationshipId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        b.Property(x => x.StudentId)
            .HasConversion(x => x.Value, x => new PersonId(x))
            .HasColumnName("student_id");
        b.Property(x => x.PersonOrOrganizationId).HasColumnName("party_id");
        b.Property(x => x.PartyKind)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("party_kind");
        b.Property(x => x.DisplayName).HasMaxLength(200).HasColumnName("display_name");
        b.Property(x => x.Email).HasMaxLength(254).HasColumnName("email");
        b.Property(x => x.Phone).HasMaxLength(40).HasColumnName("phone");
        b.Property(x => x.RelationshipType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasColumnName("relationship_type");
        b.Property(x => x.Permissions).HasConversion<int>().HasColumnName("permissions");
        b.Property(x => x.FinancialScope).HasConversion<int>().HasColumnName("financial_scope");
        b.Property(x => x.CommunicationScope)
            .HasConversion<int>()
            .HasColumnName("communication_scope");
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
        b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.IsPrimaryPayer).HasColumnName("is_primary_payer");
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
        b.Property(x => x.StatusReason).HasMaxLength(500).HasColumnName("status_reason");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.Status,
            })
            .HasDatabaseName("ix_student_relationships_owner_student_status");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.PersonOrOrganizationId,
                x.RelationshipType,
            })
            .HasDatabaseName("ix_student_relationships_owner_party_type");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.IsPrimaryPayer,
            })
            .IsUnique()
            .HasFilter("is_primary_payer = TRUE AND status = 'Active'")
            .HasDatabaseName("ux_student_relationships_primary_payer");
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
