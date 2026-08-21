using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
{
    public void Configure(EntityTypeBuilder<TrainingSession> b)
    {
        b.ToTable("training_sessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new TrainingSessionId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.StudentOwnerOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.PerformingOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.SourceBookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.TrainingPathId).HasConversion(x => x.Value, x => new TrainingPathId(x)).IsRequired();
        b.Property(x => x.InstructorId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.BranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        b.Property(x => x.ReadinessCheckedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ReadyInstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ReadyBranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        b.Property(x => x.ActualInstructorId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ActualBranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        b.Property(x => x.CompletedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CancellationId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new SessionCancellationId(x.Value) : null);
        b.Property(x => x.CancelledByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.CompletionRequestFingerprint).HasMaxLength(64);
        b.Property(x => x.StartedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.StartRequestFingerprint).HasMaxLength(64);
        b.Property(x => x.CurrentAttendanceId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingSessionAttendanceId(x.Value) : null);
        b.HasMany(x => x.AttendanceHistory).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Interventions).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Observations).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Interruptions).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.OdometerReadings).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.EnergyEntries).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.CompetencyAssessments).WithOne().HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Report).WithOne().HasForeignKey<SessionReport>(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Report).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Property(x => x.TrainingCategory).HasMaxLength(80);
        b.Property(x => x.Objectives).HasMaxLength(2000);
        b.Property(x => x.MeetingPoint).HasMaxLength(500);
        b.Property(x => x.PricingReference).HasMaxLength(200);
        b.Property(x => x.TrainingCreditAccountId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingCreditAccountId(x.Value) : null);
        b.Property(x => x.CreditReservationReference).HasMaxLength(200);
        b.HasIndex(x => new { x.OrganizationId, x.SourceBookingId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.PlannedStartAtUtc });
        b.HasIndex(x => new { x.OrganizationId, x.Status, x.ActualStartAtUtc });
    }
}
