using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Configurations;

internal sealed class ExamConvocationConfiguration : IEntityTypeConfiguration<ExamConvocation>
{
    public void Configure(EntityTypeBuilder<ExamConvocation> builder)
    {
        builder.ToTable("exam_convocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamConvocationId(x)).ValueGeneratedNever();
        builder.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        builder.Property(x => x.RegistrationId).HasColumnName("registration_id").HasConversion(x => x.Value, x => new ExamRegistrationId(x));
        builder.Property(x => x.StudentId).HasColumnName("student_id").HasConversion(x => x.Value, x => new PersonId(x));
        builder.Property(x => x.CurrentVersion).HasColumnName("current_version");
        builder.Property(x => x.DeliveryStatus).HasColumnName("delivery_status").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.DeliveryChannel).HasColumnName("delivery_channel").HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.DeliveredAtUtc).HasColumnName("delivered_at_utc");
        builder.Property(x => x.DeliveredByUserId).HasColumnName("delivered_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.AcknowledgedAtUtc).HasColumnName("acknowledged_at_utc");
        builder.Property(x => x.InternalMeetingAtUtc).HasColumnName("internal_meeting_at_utc");
        builder.Property(x => x.InternalMeetingInstructions).HasColumnName("internal_meeting_instructions").HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        builder.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        builder.HasIndex(x => new { x.OrganizationId, x.RegistrationId }).IsUnique().HasDatabaseName("ux_exam_convocation_registration");
        builder.HasIndex(x => new { x.OrganizationId, x.StudentId }).HasDatabaseName("ix_exam_convocation_student");
        builder.HasMany(x => x.Revisions)
            .WithOne()
            .HasForeignKey(x => x.ConvocationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Revisions)
            .HasField("_revisions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.CurrentRevision);
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class ExamConvocationRevisionConfiguration : IEntityTypeConfiguration<ExamConvocationRevision>
{
    public void Configure(EntityTypeBuilder<ExamConvocationRevision> builder)
    {
        builder.ToTable("exam_convocation_revisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new ExamConvocationRevisionId(x)).ValueGeneratedNever();
        builder.Property(x => x.ConvocationId).HasColumnName("convocation_id").HasConversion(x => x.Value, x => new ExamConvocationId(x));
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.ExamCenterId).HasColumnName("exam_center_id").HasConversion(x => x.Value, x => new ExamCenterId(x));
        builder.Property(x => x.CenterName).HasColumnName("center_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CenterAddress).HasColumnName("center_address").HasMaxLength(1000);
        builder.Property(x => x.TimeZoneId).HasColumnName("time_zone_id").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ScheduledStartUtc).HasColumnName("scheduled_start_utc");
        builder.Property(x => x.ScheduledEndUtc).HasColumnName("scheduled_end_utc");
        builder.Property(x => x.ProviderCode).HasColumnName("provider_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.OfficialReference).HasColumnName("official_reference").HasMaxLength(200);
        builder.Property(x => x.CandidateReference).HasColumnName("candidate_reference").HasMaxLength(200);
        builder.Property(x => x.Instructions).HasColumnName("instructions").HasMaxLength(4000);
        builder.Property(x => x.RequiredDocuments).HasColumnName("required_documents").HasMaxLength(4000);
        builder.Property(x => x.ProviderPayloadReference).HasColumnName("provider_payload_reference").HasMaxLength(500);
        builder.Property(x => x.OperationId).HasColumnName("operation_id");
        builder.Property(x => x.RequestFingerprint).HasColumnName("request_fingerprint").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).HasColumnName("received_at_utc");
        builder.HasIndex(x => new { x.ConvocationId, x.Version }).IsUnique().HasDatabaseName("ux_exam_convocation_revision_version");
        builder.HasIndex(x => new { x.ConvocationId, x.OperationId }).IsUnique().HasDatabaseName("ux_exam_convocation_revision_operation");
    }
}
