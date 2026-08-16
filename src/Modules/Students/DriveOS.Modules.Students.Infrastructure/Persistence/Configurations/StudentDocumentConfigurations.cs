using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class StudentDocumentConfiguration : IEntityTypeConfiguration<StudentDocument>
{
    public void Configure(EntityTypeBuilder<StudentDocument> b)
    {
        b.ToTable("student_documents");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new StudentDocumentId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.EnrollmentId)
            .HasColumnName("enrollment_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new DraftEnrollmentId(x.Value) : null
            );
        b.Property(x => x.DocumentType).HasColumnName("document_type").HasMaxLength(100);
        b.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(x => x.Visibility).HasColumnName("visibility").HasConversion<int>();
        b.Property(x => x.ExpiresOn).HasColumnName("expires_on");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.CurrentVersion).HasColumnName("current_version");
        b.Property(x => x.RequestedAtUtc).HasColumnName("requested_at_utc");
        b.Property(x => x.RequestedByUserId)
            .HasColumnName("requested_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.StudentId,
                x.Status,
            })
            .HasDatabaseName("ix_student_documents_owner_student_status");
        b.HasIndex(x => x.ExpiresOn);
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Enrollment>()
            .WithMany()
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Versions)
            .WithOne()
            .HasForeignKey(x => x.StudentDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.AccessLogs)
            .WithOne()
            .HasForeignKey(x => x.StudentDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class StudentDocumentVersionConfiguration
    : IEntityTypeConfiguration<StudentDocumentVersion>
{
    public void Configure(EntityTypeBuilder<StudentDocumentVersion> b)
    {
        b.ToTable("student_document_versions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.StudentDocumentId).HasColumnName("document_id").HasConversion(x => x.Value, x => new StudentDocumentId(x));
        b.Property(x => x.VersionNumber).HasColumnName("version_number");
        b.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255);
        b.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(150);
        b.Property(x => x.SizeBytes).HasColumnName("size_bytes");
        b.Property(x => x.Checksum).HasColumnName("checksum").HasMaxLength(64);
        b.Property(x => x.StorageReference).HasColumnName("storage_reference").HasMaxLength(500);
        b.Property(x => x.IsCurrent).HasColumnName("is_current");
        b.Property(x => x.UploadedAtUtc).HasColumnName("uploaded_at_utc");
        b.Property(x => x.UploadedByUserId)
            .HasColumnName("uploaded_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.ReviewedAtUtc).HasColumnName("reviewed_at_utc");
        b.Property(x => x.ReviewedByUserId)
            .HasColumnName("reviewed_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.ReplacedAtUtc).HasColumnName("replaced_at_utc");
        b.HasIndex(x => new { x.StudentDocumentId, x.VersionNumber }).IsUnique();
        b.HasIndex(x => new { x.StudentDocumentId, x.IsCurrent })
            .IsUnique()
            .HasFilter("is_current = TRUE");
    }
}

internal sealed class StudentDocumentAccessLogConfiguration
    : IEntityTypeConfiguration<StudentDocumentAccessLog>
{
    public void Configure(EntityTypeBuilder<StudentDocumentAccessLog> b)
    {
        b.ToTable("student_document_access_logs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.StudentDocumentId).HasColumnName("document_id").HasConversion(x => x.Value, x => new StudentDocumentId(x));
        b.Property(x => x.VersionId).HasColumnName("version_id");
        b.Property(x => x.Action).HasColumnName("action").HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc");
        b.HasIndex(x => new { x.StudentDocumentId, x.OccurredAtUtc });
    }
}
