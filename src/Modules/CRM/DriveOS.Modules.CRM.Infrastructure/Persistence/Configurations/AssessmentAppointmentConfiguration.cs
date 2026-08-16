using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class AssessmentAppointmentConfiguration
    : IEntityTypeConfiguration<AssessmentAppointment>
{
    public void Configure(EntityTypeBuilder<AssessmentAppointment> b)
    {
        b.ToTable("assessment_appointments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(x => x.Value, x => new AssessmentAppointmentId(x))
            .ValueGeneratedNever();
        b.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.LeadId)
            .HasColumnName("lead_id")
            .HasConversion(x => x.Value, x => new LeadId(x));
        b.Property(x => x.BranchId)
            .HasColumnName("branch_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new BranchId(x.Value) : null
            );
        b.Property(x => x.StartsAtUtc).HasColumnName("starts_at_utc");
        b.Property(x => x.EndsAtUtc).HasColumnName("ends_at_utc");
        b.Property(x => x.Type)
            .HasColumnName("assessment_type")
            .HasConversion<string>()
            .HasMaxLength(32);
        b.Property(x => x.DeliveryMode)
            .HasColumnName("delivery_mode")
            .HasConversion<string>()
            .HasMaxLength(20);
        b.Property(x => x.LocationKind)
            .HasColumnName("location_kind")
            .HasConversion<string>()
            .HasMaxLength(24);
        b.Property(x => x.LocationDetails).HasColumnName("location_details").HasMaxLength(500);
        b.Property(x => x.EvaluatorUserId)
            .HasColumnName("evaluator_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.VehicleId).HasColumnName("vehicle_id");
        b.Property(x => x.RoomId).HasColumnName("room_id");
        b.Property(x => x.SimulatorId).HasColumnName("simulator_id");
        b.Property(x => x.PriceAmount).HasColumnName("price_amount").HasPrecision(18, 2);
        b.Property(x => x.PriceCurrency).HasColumnName("price_currency").HasMaxLength(3);
        b.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ClosedAtUtc).HasColumnName("closed_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null
            );
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.Status,
                x.StartsAtUtc,
            })
            .HasDatabaseName("ix_assessment_appointments_org_status_start");
        b.HasIndex(x => new { x.OrganizationId, x.LeadId })
            .HasDatabaseName("ix_assessment_appointments_org_lead");
        b.Ignore(x => x.DomainEvents);
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.EvaluatorUserId,
                x.StartsAtUtc,
            })
            .HasDatabaseName("ix_assessment_appointments_org_evaluator_start");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.VehicleId,
                x.StartsAtUtc,
            })
            .HasDatabaseName("ix_assessment_appointments_org_vehicle_start");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.RoomId,
                x.StartsAtUtc,
            })
            .HasDatabaseName("ix_assessment_appointments_org_room_start");
        b.HasIndex(x => new
            {
                x.OrganizationId,
                x.SimulatorId,
                x.StartsAtUtc,
            })
            .HasDatabaseName("ix_assessment_appointments_org_simulator_start");
    }
}
