using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamRegistrationFileConfiguration : IEntityTypeConfiguration<ExamRegistrationFile>
{
    public void Configure(EntityTypeBuilder<ExamRegistrationFile> builder)
    {
        builder.ToTable("exam_registration_files");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamRegistrationFileId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CurrentVersion).HasColumnName("current_version");
        builder.Property(x => x.LastEvaluatedAtUtc).HasColumnName("last_evaluated_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique();
        builder.HasMany(x => x.Revisions).WithOne().HasForeignKey(x => x.RegistrationFileId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.CurrentRevision);
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ExamRegistrationFileRevisionConfiguration : IEntityTypeConfiguration<ExamRegistrationFileRevision>
{
    public void Configure(EntityTypeBuilder<ExamRegistrationFileRevision> builder)
    {
        builder.ToTable("exam_registration_file_revisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.RegistrationFileId).HasColumnName("registration_file_id").HasConversion(x => x.Value, x => new ExamRegistrationFileId(x));
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.CandidateReference).HasColumnName("candidate_reference").HasMaxLength(200);
        builder.Property(x => x.OfficialDataJson).HasColumnName("official_data_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.Value, x => new UserId(x));
        builder.HasIndex(x => new { x.RegistrationFileId, x.Version }).IsUnique();
        builder.HasMany(x => x.Checklist).WithOne().HasForeignKey(x => x.RevisionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Checklist).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ExamRegistrationChecklistItemSnapshotConfiguration : IEntityTypeConfiguration<ExamRegistrationChecklistItemSnapshot>
{
    public void Configure(EntityTypeBuilder<ExamRegistrationChecklistItemSnapshot> builder)
    {
        builder.ToTable("exam_registration_file_checklist_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.RevisionId).HasColumnName("revision_id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Required).HasColumnName("required");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.MessageKey).HasColumnName("message_key").HasMaxLength(250).IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(100);
        builder.Property(x => x.Evidence).HasColumnName("evidence").HasMaxLength(2000);
        builder.HasIndex(x => new { x.RevisionId, x.Code }).IsUnique();
    }
}
