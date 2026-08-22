using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamPreparationConfiguration : IEntityTypeConfiguration<ExamPreparation>
{
    public void Configure(EntityTypeBuilder<ExamPreparation> b)
    {
        b.ToTable("exam_preparations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ExamPreparationId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.RegistrationId).HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ConfirmedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.Status).HasConversion<int>();
        b.Ignore(x => x.IsConfirmed);
        b.Property(x => x.LastRequestFingerprint).HasMaxLength(128);
        b.Ignore(x => x.ReminderOffsetsDays);
        b.Property<List<int>>("_reminderOffsetsDays").HasColumnName("reminder_offsets_days").HasColumnType("integer[]");
        b.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique();
        b.HasMany(x => x.Checks).WithOne().HasForeignKey(x => x.PreparationId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Checks).HasField("_checks").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ExamPreparationCheckConfiguration : IEntityTypeConfiguration<ExamPreparationCheck>
{
    public void Configure(EntityTypeBuilder<ExamPreparationCheck> b)
    {
        b.ToTable("exam_preparation_checks");
        b.HasKey(x => x.Id);
        b.Property(x => x.PreparationId).HasConversion(x => x.Value, x => new ExamPreparationId(x));
        b.Property(x => x.Code).HasMaxLength(120).IsRequired();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.MessageKey).HasMaxLength(250).IsRequired();
        b.Property(x => x.Source).HasMaxLength(120).IsRequired();
        b.Property(x => x.Evidence).HasMaxLength(1000);
        b.HasIndex(x => new { x.PreparationId, x.Code }).IsUnique();
    }
}
