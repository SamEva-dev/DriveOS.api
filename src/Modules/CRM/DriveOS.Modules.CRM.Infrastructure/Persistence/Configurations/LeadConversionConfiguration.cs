using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Configurations;

internal sealed class LeadConversionConfiguration : IEntityTypeConfiguration<LeadConversion>
{
    public void Configure(EntityTypeBuilder<LeadConversion> b)
    {
        b.ToTable("lead_conversions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new LeadConversionId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.LeadId).HasColumnName("lead_id").HasConversion(x => x.Value, x => new LeadId(x));
        b.Property(x => x.AcceptedOfferId).HasColumnName("accepted_offer_id").HasConversion(x => x.Value, x => new CommercialOfferId(x));
        b.Property(x => x.BranchId).HasColumnName("branch_id").HasConversion(x => x.Value, x => new BranchId(x));
        b.Property(x => x.ResponsibleUserId).HasColumnName("responsible_user_id").HasConversion(x => x.Value, x => new UserId(x));
        b.Property(x => x.TrainingCode).HasColumnName("training_code").HasMaxLength(100);
        b.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
        b.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(254);
        b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(40);
        b.Property(x => x.IdentityVerified).HasColumnName("identity_verified");
        b.Property(x => x.ConsentsVerified).HasColumnName("consents_verified");
        b.Property(x => x.DuplicateCheckCompleted).HasColumnName("duplicate_check_completed");
        b.Property(x => x.GuardianSummary).HasColumnName("guardian_summary").HasMaxLength(2000);
        b.Property(x => x.PayerSummary).HasColumnName("payer_summary").HasMaxLength(2000);
        b.Property(x => x.RequiredDocumentCodes).HasColumnName("required_document_codes").HasMaxLength(2000);
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.StudentPersonId).HasColumnName("student_person_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PersonId(x.Value) : null);
        b.Property(x => x.StudentEnrollmentId)
            .HasColumnName("student_enrollment_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new DraftEnrollmentId(x.Value) : null);
        b.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.HasIndex(x => new { x.OrganizationId, x.LeadId }).IsUnique().HasDatabaseName("ux_lead_conversions_organization_lead");
        b.Ignore(x => x.DomainEvents);
    }
}
