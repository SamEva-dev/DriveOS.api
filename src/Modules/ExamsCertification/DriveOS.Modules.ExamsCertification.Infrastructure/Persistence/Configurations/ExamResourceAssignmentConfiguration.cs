using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamResourceAssignmentConfiguration : IEntityTypeConfiguration<ExamResourceAssignment>
{
    public void Configure(EntityTypeBuilder<ExamResourceAssignment> b)
    {
        b.ToTable("exam_resource_assignments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new ExamResourceAssignmentId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.RegistrationId).HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.OperationalPlanId).HasConversion(x => x.Value, x => new ExamOperationalPlanId(x));
        b.Property(x => x.InstructorCalendarResourceId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new CalendarResourceId(x.Value) : null);
        b.Property(x => x.InstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.VehicleCalendarResourceId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new CalendarResourceId(x.Value) : null);
        b.Property(x => x.VehicleId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new VehicleId(x.Value) : null);
        b.Property(x => x.SchedulingBookingId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BookingId(x.Value) : null);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.RequestFingerprint).HasMaxLength(128).IsRequired();
        b.Property(x => x.SchedulingErrorCode).HasMaxLength(200);
        b.Property(x => x.Status).HasConversion<int>();
        b.Ignore(x => x.InstructorWarnings);
        b.Ignore(x => x.VehicleExternalReviews);
        b.Property<List<string>>("_instructorWarnings").HasColumnName("instructor_warnings").HasColumnType("text[]");
        b.Property<List<string>>("_vehicleExternalReviews").HasColumnName("vehicle_external_reviews").HasColumnType("text[]");
        b.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique();
    }
}
