using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;
internal sealed class ExamSuccessProcessConfiguration : IEntityTypeConfiguration<ExamSuccessProcess>
{
    public void Configure(EntityTypeBuilder<ExamSuccessProcess> builder)
    {
        builder.ToTable("exam_success_processes"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamSuccessProcessId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.ExamResultId).HasColumnName("exam_result_id").HasConversion(x => x.Value, x => new ExamResultId(x));
        builder.Property(x => x.ResultRevision).HasColumnName("result_revision");
        builder.Property(x => x.AttemptId).HasColumnName("attempt_id").HasConversion(x => x.Value, x => new ExamAttemptId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.CompletedByUserId).HasColumnName("completed_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.SupersededAtUtc).HasColumnName("superseded_at_utc");
        builder.Property(x => x.ArchivedAtUtc).HasColumnName("archived_at_utc");
        builder.Property(x => x.ArchivedByUserId).HasColumnName("archived_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.ExamResultId, x.ResultRevision }).IsUnique().HasDatabaseName("ux_exam_success_process_result_revision");
        builder.OwnsMany(x => x.Actions, action =>
        {
            action.ToTable("exam_success_actions"); action.WithOwner().HasForeignKey("success_process_id"); action.HasKey(x => x.Id);
            action.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); action.Property(x => x.Code).HasColumnName("code").HasConversion<string>().HasMaxLength(64);
            action.Property(x => x.Blocking).HasColumnName("blocking"); action.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
            action.Property(x => x.EvidenceReference).HasColumnName("evidence_reference").HasMaxLength(1000); action.Property(x => x.ReasonCode).HasColumnName("reason_code").HasMaxLength(200);
            action.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(2000); action.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
            action.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
            action.HasIndex("success_process_id", nameof(ExamSuccessAction.Code)).IsUnique();
        });
        builder.Ignore(x => x.DomainEvents);
    }
}
