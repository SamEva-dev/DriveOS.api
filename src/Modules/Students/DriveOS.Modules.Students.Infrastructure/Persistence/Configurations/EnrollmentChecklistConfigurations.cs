using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class EnrollmentChecklistConfiguration
    : IEntityTypeConfiguration<EnrollmentChecklist>
{
    public void Configure(EntityTypeBuilder<EnrollmentChecklist> b)
    {
        b.ToTable("enrollment_checklists");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new EnrollmentChecklistId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        b.Property(x => x.StudentId)
            .HasConversion(x => x.Value, x => new PersonId(x))
            .HasColumnName("student_id");
        b.Property(x => x.EnrollmentId)
            .HasConversion(x => x.Value, x => new DraftEnrollmentId(x))
            .HasColumnName("enrollment_id");
        b.HasIndex(x => new { x.OrganizationId, x.EnrollmentId })
            .IsUnique()
            .HasDatabaseName("ux_enrollment_checklists_owner_enrollment");
        b.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Enrollment>()
            .WithOne()
            .HasForeignKey<EnrollmentChecklist>(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.EnrollmentChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class EnrollmentChecklistItemConfiguration
    : IEntityTypeConfiguration<EnrollmentChecklistItem>
{
    public void Configure(EntityTypeBuilder<EnrollmentChecklistItem> b)
    {
        b.ToTable("enrollment_checklist_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EnrollmentChecklistId).HasColumnName("checklist_id").HasConversion(x => x.Value, x => new EnrollmentChecklistId(x));
        b.Property(x => x.RuleId).HasColumnName("rule_id");
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
        b.Property(x => x.LabelKey).HasColumnName("label_key").HasMaxLength(200);
        b.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(x => x.IsBlocking).HasColumnName("is_blocking");
        b.Property(x => x.TargetRoute).HasColumnName("target_route").HasMaxLength(300);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ResponsibleUserId).HasColumnName("responsible_user_id");
        b.Property(x => x.DueAtUtc).HasColumnName("due_at_utc");
        b.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(500);
        b.Property(x => x.ReminderCount).HasColumnName("reminder_count");
        b.Property(x => x.LastReminderAtUtc).HasColumnName("last_reminder_at_utc");
        b.Property(x => x.ModifiedByUserId)
            .HasColumnName("modified_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.ModifiedAtUtc).HasColumnName("modified_at_utc");
        b.HasIndex(x => new { x.EnrollmentChecklistId, x.RuleId }).IsUnique();
    }
}

internal sealed class EnrollmentChecklistRuleConfiguration
    : IEntityTypeConfiguration<EnrollmentChecklistRule>
{
    public void Configure(EntityTypeBuilder<EnrollmentChecklistRule> b)
    {
        b.ToTable("enrollment_checklist_rules");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .HasColumnName("organization_id");
        b.Property(x => x.TrainingCode).HasColumnName("training_code").HasMaxLength(100);
        b.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
        b.Property(x => x.LabelKey).HasColumnName("label_key").HasMaxLength(200);
        b.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(40);
        b.Property(x => x.IsBlocking).HasColumnName("is_blocking");
        b.Property(x => x.TargetRoute).HasColumnName("target_route").HasMaxLength(300);
        b.Property(x => x.DueInDays).HasColumnName("due_in_days");
        b.Property(x => x.IsActive).HasColumnName("is_active");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.TrainingCode,
                x.Code,
            })
            .IsUnique()
            .HasDatabaseName("ux_enrollment_checklist_rules_scope_code");
    }
}
