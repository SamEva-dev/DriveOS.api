using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Students.Infrastructure.Persistence.Configurations;

internal sealed class EnrollmentReactivationConfiguration
    : IEntityTypeConfiguration<EnrollmentReactivation>
{
    public void Configure(EntityTypeBuilder<EnrollmentReactivation> b)
    {
        b.ToTable("enrollment_reactivations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new EnrollmentReactivationId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.StudentId)
            .HasColumnName("student_id")
            .HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.EnrollmentId)
            .HasColumnName("enrollment_id")
            .HasConversion(x => x.Value, x => new DraftEnrollmentId(x));
        b.Property(x => x.SuspensionId)
            .HasColumnName("suspension_id")
            .HasConversion(x => x.Value, x => new EnrollmentSuspensionId(x));
        b.Property(x => x.Mode).HasColumnName("mode").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ResumeDate).HasColumnName("resume_date");
        b.Property(x => x.Conditions).HasColumnName("conditions").HasMaxLength(2000);
        b.Property(x => x.PedagogyReviewRequested).HasColumnName("pedagogy_review_requested");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.AppliedAtUtc).HasColumnName("applied_at_utc");
        b.HasOne<EnrollmentSuspension>()
            .WithMany()
            .HasForeignKey(x => x.SuspensionId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DriveOS.Modules.Students.Domain.Enrollments.Enrollment>()
            .WithMany()
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<DriveOS.Modules.Students.Domain.Students.Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Checks)
            .WithOne()
            .HasForeignKey(x => x.EnrollmentReactivationId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Checks).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new
        {
            x.OrganizationId,
            x.StudentId,
            x.Status,
        });
        b.HasIndex(x => new { x.Status, x.ResumeDate });
        b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class EnrollmentReactivationCheckConfiguration
    : IEntityTypeConfiguration<EnrollmentReactivationCheck>
{
    public void Configure(EntityTypeBuilder<EnrollmentReactivationCheck> b)
    {
        b.ToTable("enrollment_reactivation_checks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EnrollmentReactivationId)
            .HasColumnName("enrollment_reactivation_id")
            .HasConversion(x => x.Value, x => new EnrollmentReactivationId(x));
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Detail).HasColumnName("detail").HasMaxLength(1000);
        b.HasIndex(x => new { x.EnrollmentReactivationId, x.Type }).IsUnique();
    }
}
