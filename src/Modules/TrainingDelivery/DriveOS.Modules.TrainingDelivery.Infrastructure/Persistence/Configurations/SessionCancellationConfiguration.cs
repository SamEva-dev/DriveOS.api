using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Configurations;

internal sealed class SessionCancellationConfiguration : IEntityTypeConfiguration<SessionCancellation>
{
    public void Configure(EntityTypeBuilder<SessionCancellation> b)
    {
        b.ToTable("session_cancellations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new SessionCancellationId(x));
        b.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.TrainingSessionId).HasConversion(x => x.Value, x => new TrainingSessionId(x)).IsRequired();
        b.Property(x => x.SourceBookingId).HasConversion(x => x.Value, x => new BookingId(x)).IsRequired();
        b.Property(x => x.StudentOwnerOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.PerformingOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.StudentId).HasConversion(x => x.Value, x => new PersonId(x)).IsRequired();
        b.Property(x => x.InstructorId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.BranchId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new BranchId(x.Value) : null);
        b.Property(x => x.CancelledByUserId).HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.Property(x => x.CreatedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.ReasonDetails).HasMaxLength(3000);
        b.Property(x => x.DecisionReason).HasMaxLength(2000);
        b.Property(x => x.TrainingCreditAccountId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new TrainingCreditAccountId(x.Value) : null);
        b.Property(x => x.CreditReservationReference).HasMaxLength(200);
        b.Property(x => x.PricingReference).HasMaxLength(200);
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.OrganizationId, x.TrainingSessionId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique();
        b.HasIndex(x => new { x.OrganizationId, x.CancelledAtUtc });
    }
}
