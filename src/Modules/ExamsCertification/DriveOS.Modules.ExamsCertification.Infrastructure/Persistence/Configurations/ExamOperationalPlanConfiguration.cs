using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamOperationalPlanConfiguration : IEntityTypeConfiguration<ExamOperationalPlan>
{
    public void Configure(EntityTypeBuilder<ExamOperationalPlan> builder)
    {
        builder.ToTable("exam_operational_plans"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamOperationalPlanId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.ConvocationVersion).HasColumnName("convocation_version");
        builder.Property(x => x.OfficialStartUtc).HasColumnName("official_start_utc"); builder.Property(x => x.OfficialEndUtc).HasColumnName("official_end_utc");
        builder.Property(x => x.MeetingAtUtc).HasColumnName("meeting_at_utc"); builder.Property(x => x.OperationalWindowStartUtc).HasColumnName("operational_window_start_utc");
        builder.Property(x => x.OperationalWindowEndUtc).HasColumnName("operational_window_end_utc"); builder.Property(x => x.TravelBufferBeforeMinutes).HasColumnName("travel_buffer_before_minutes");
        builder.Property(x => x.TravelBufferAfterMinutes).HasColumnName("travel_buffer_after_minutes");
        builder.Property(x => x.DepartureBranchId).HasColumnName("departure_branch_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        builder.Property(x => x.InstructorRequired).HasColumnName("instructor_required"); builder.Property(x => x.VehicleRequired).HasColumnName("vehicle_required");
        builder.Property(x => x.MeetingInstructions).HasColumnName("meeting_instructions").HasMaxLength(2000); builder.Property(x => x.HasSchedulingConflicts).HasColumnName("has_scheduling_conflicts");
        builder.Property(x => x.InstructorCandidatesAvailable).HasColumnName("instructor_candidates_available"); builder.Property(x => x.VehicleCandidatesAvailable).HasColumnName("vehicle_candidates_available");
        builder.Property(x => x.ConflictSummary).HasColumnName("conflict_summary").HasMaxLength(2000); builder.Property(x => x.LastAssessedAtUtc).HasColumnName("last_assessed_at_utc");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40); builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique().HasDatabaseName("ux_exam_operational_plan_registration");
        builder.HasIndex(x => new { x.OrganizationId, x.OfficialStartUtc }).HasDatabaseName("ix_exam_operational_plan_start");
        builder.Ignore(x => x.DomainEvents);
    }
}
