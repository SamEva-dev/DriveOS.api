using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentIdentityAuditEntryConfiguration
    : IEntityTypeConfiguration<StudentIdentityAuditEntry>
{
    public void Configure(EntityTypeBuilder<StudentIdentityAuditEntry> b)
    {
        b.ToTable("student_identity_audit");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.Action).HasColumnName("action").HasMaxLength(40).IsRequired();
        b.Property(x => x.Justification)
            .HasColumnName("justification")
            .HasMaxLength(500)
            .IsRequired();
        b.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.OccurredAtUtc,
            })
            .HasDatabaseName("ix_student_identity_audit_owner_date");
    }
}
