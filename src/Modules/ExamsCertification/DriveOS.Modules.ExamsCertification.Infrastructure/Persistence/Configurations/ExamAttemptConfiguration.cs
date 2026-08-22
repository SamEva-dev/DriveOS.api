using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> b)
    {
        b.ToTable("exam_attempts"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ExamAttemptId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.RegistrationId).HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        b.Property(x => x.PreparationId).HasConversion(x => x.Value, x => new ExamPreparationId(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.ExamCenterId).HasConversion(x => x.Value, x => new ExamCenterId(x));
        b.Property(x => x.ExamPlaceId).HasConversion(x => x.Value, x => new ExamPlaceId(x));
        b.Property(x => x.InstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.VehicleId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new VehicleId(x.Value) : null);
        b.Property(x => x.SchedulingBookingId).HasConversion(x => x.Value, x => new BookingId(x));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.Status).HasConversion<int>(); b.Property(x => x.AttendanceStatus).HasConversion<int>();
        b.Property(x => x.ExamType).HasMaxLength(100).IsRequired(); b.Property(x => x.LicenseCategory).HasMaxLength(80).IsRequired();
        b.Property(x => x.OperationalReasonCode).HasMaxLength(120); b.Property(x => x.OperationalNotes).HasMaxLength(2000); b.Ignore(x => x.IsTerminal);
        b.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.StudentId, x.ExamType, x.LicenseCategory, x.AttemptNumber }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.Status, x.ScheduledStartUtc });
        b.HasMany(x => x.Timeline).WithOne().HasForeignKey(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Timeline).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ExamAttemptTimelineEntryConfiguration : IEntityTypeConfiguration<ExamAttemptTimelineEntry>
{
    public void Configure(EntityTypeBuilder<ExamAttemptTimelineEntry> b)
    {
        b.ToTable("exam_attempt_timeline"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ExamAttemptTimelineEntryId(x));
        b.Property(x => x.AttemptId).HasConversion(x => x.Value, x => new ExamAttemptId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.ActorUserId).HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.InstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.VehicleId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new VehicleId(x.Value) : null);
        b.Property(x => x.Type).HasConversion<int>(); b.Property(x => x.Status).HasConversion<int>(); b.Property(x => x.LocationPurpose).HasConversion<int?>();
        b.Property(x => x.RequestFingerprint).HasMaxLength(128).IsRequired(); b.Property(x => x.Note).HasMaxLength(2000);
        b.Property(x => x.Latitude).HasPrecision(9,6); b.Property(x => x.Longitude).HasPrecision(9,6); b.Property(x => x.AccuracyMeters).HasPrecision(10,2);
        b.HasIndex(x => new { x.AttemptId, x.OperationId }).IsUnique(); b.HasIndex(x => new { x.OrganizationId, x.OccurredAtUtc });
    }
}
