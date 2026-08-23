using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("employees"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasConversion(x => x.Value, x => new EmployeeId(x)).ValueGeneratedNever();
        b.Property(x => x.EmployerOrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        b.Property(x => x.PersonId).HasConversion(x => x.Value, x => new PersonId(x));
        b.Property(x => x.UserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.CreatedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.LastModifiedByUserId).HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
        b.Property(x => x.EmployeeNumber).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.RehiredFromEmployeeId).HasColumnName("rehired_from_employee_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new EmployeeId(x.Value));
        b.HasIndex(x => new { x.EmployerOrganizationId, x.EmployeeNumber }).IsUnique().HasFilter("\"Status\" <> 'Ended'");
        b.HasIndex(x => new { x.EmployerOrganizationId, x.RehiredFromEmployeeId });
        b.HasIndex(x => new { x.EmployerOrganizationId, x.PersonId, x.Status });
        b.HasIndex(x => new { x.EmployerOrganizationId, x.UserId, x.Status });
        b.HasIndex(x => new { x.EmployerOrganizationId, x.Status });

        b.OwnsMany(x => x.BranchAssignments, a =>
        {
            a.ToTable("employee_branch_assignments");
            a.WithOwner().HasForeignKey("employee_id");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new EmployeeBranchAssignmentId(x)).ValueGeneratedNever();
            a.Property(x => x.BranchId).HasColumnName("branch_id").HasConversion(x => x.Value, x => new BranchId(x)).IsRequired();
            a.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
            a.Property(x => x.EndDate).HasColumnName("end_date");
            a.Property(x => x.IsPrimary).HasColumnName("is_primary").IsRequired();
            a.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
            a.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            a.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
            a.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
            a.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
            a.HasIndex("employee_id", nameof(EmployeeBranchAssignment.BranchId), nameof(EmployeeBranchAssignment.StartDate));
            a.HasIndex("employee_id", nameof(EmployeeBranchAssignment.IsPrimary), nameof(EmployeeBranchAssignment.StartDate), nameof(EmployeeBranchAssignment.EndDate));
        });
        b.Navigation(x => x.BranchAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(x => x.JobPositionAssignments, a =>
        {
            a.ToTable("employee_job_position_assignments");
            a.WithOwner().HasForeignKey("employee_id");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new EmployeeJobPositionAssignmentId(x)).ValueGeneratedNever();
            a.Property(x => x.JobPositionId).HasColumnName("job_position_id").HasConversion(x => x.Value, x => new JobPositionId(x)).IsRequired();
            a.Property(x => x.BranchId).HasColumnName("branch_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new BranchId(x.Value));
            a.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
            a.Property(x => x.EndDate).HasColumnName("end_date");
            a.Property(x => x.IsPrimary).HasColumnName("is_primary").IsRequired();
            a.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24).IsRequired();
            a.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            a.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
            a.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
            a.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
            a.HasIndex("employee_id", nameof(EmployeeJobPositionAssignment.JobPositionId), nameof(EmployeeJobPositionAssignment.StartDate));
            a.HasIndex("employee_id", nameof(EmployeeJobPositionAssignment.IsPrimary), nameof(EmployeeJobPositionAssignment.StartDate), nameof(EmployeeJobPositionAssignment.EndDate));
            a.HasIndex("employee_id", nameof(EmployeeJobPositionAssignment.BranchId), nameof(EmployeeJobPositionAssignment.StartDate), nameof(EmployeeJobPositionAssignment.EndDate));
        });
        b.Navigation(x => x.JobPositionAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);


        b.OwnsMany(x => x.Qualifications, q =>
        {
            q.ToTable("employee_qualifications"); q.WithOwner().HasForeignKey("employee_id"); q.HasKey(x => x.Id);
            q.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new EmployeeQualificationId(x)).ValueGeneratedNever();
            q.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
            q.Property(x => x.QualificationType).HasColumnName("qualification_type").HasMaxLength(64).IsRequired();
            q.Property(x => x.Title).HasColumnName("title").HasMaxLength(160).IsRequired();
            q.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(120); q.Property(x => x.IssuingAuthority).HasColumnName("issuing_authority").HasMaxLength(160);
            q.Property(x => x.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(32).IsRequired(); q.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
            q.Property(x => x.DeclaredByUserId).HasColumnName("declared_by_user_id").HasConversion(x => x.Value, x => new UserId(x));
            q.Property(x => x.VerifiedByUserId).HasColumnName("verified_by_user_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
            q.Property(x => x.SupersededById).HasColumnName("superseded_by_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new EmployeeQualificationId(x.Value));
            q.HasIndex("employee_id", nameof(EmployeeQualification.CountryCode), nameof(EmployeeQualification.QualificationType), nameof(EmployeeQualification.Status));
        });
        b.Navigation(x => x.Qualifications).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(x => x.InstructorAuthorizations, a =>
        {
            a.ToTable("employee_instructor_authorizations"); a.WithOwner().HasForeignKey("employee_id"); a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new InstructorAuthorizationId(x)).ValueGeneratedNever();
            a.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired(); a.Property(x => x.AuthorizationType).HasColumnName("authorization_type").HasMaxLength(64).IsRequired();
            a.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(120).IsRequired(); a.Property(x => x.IssuingAuthority).HasColumnName("issuing_authority").HasMaxLength(160).IsRequired();
            a.Property(x => x.JurisdictionCode).HasColumnName("jurisdiction_code").HasMaxLength(32); a.Property(x => x.LicenseCategoryCode).HasColumnName("license_category_code").HasMaxLength(32).IsRequired();
            a.Property(x => x.Source).HasColumnName("source").HasConversion<string>().HasMaxLength(32).IsRequired(); a.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
            a.Property(x => x.DeclaredByUserId).HasColumnName("declared_by_user_id").HasConversion(x => x.Value, x => new UserId(x));
            a.Property(x => x.VerifiedByUserId).HasColumnName("verified_by_user_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
            a.Property(x => x.SupersededById).HasColumnName("superseded_by_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new InstructorAuthorizationId(x.Value));
            a.HasIndex("employee_id", nameof(InstructorAuthorization.CountryCode), nameof(InstructorAuthorization.AuthorizationType), nameof(InstructorAuthorization.LicenseCategoryCode), nameof(InstructorAuthorization.Status));
        });
        b.Navigation(x => x.InstructorAuthorizations).UsePropertyAccessMode(PropertyAccessMode.Field);


        b.OwnsMany(x => x.EmploymentContracts, c =>
        {
            c.ToTable("employee_employment_contracts");
            c.WithOwner().HasForeignKey("employee_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new EmploymentContractId(x)).ValueGeneratedNever();
            c.Property(x => x.ContractType).HasColumnName("contract_type").HasConversion<string>().HasMaxLength(40).IsRequired();
            c.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
            c.Property(x => x.EndDate).HasColumnName("end_date");
            c.Property(x => x.ContractualWeeklyHours).HasColumnName("contractual_weekly_hours").HasPrecision(6, 2);
            c.Property(x => x.PrimaryJobPositionId).HasColumnName("primary_job_position_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new JobPositionId(x.Value));
            c.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
            c.Property(x => x.ContractDocumentId).HasColumnName("contract_document_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new ContractDocumentId(x.Value));
            c.Property(x => x.SignatureProcessId).HasColumnName("signature_process_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new SignatureProcessId(x.Value));
            c.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            c.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
            c.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
            c.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x == null ? (Guid?)null : x.Value.Value, x => x == null ? null : new UserId(x.Value));
            c.HasIndex("employee_id", nameof(EmploymentContract.StartDate), nameof(EmploymentContract.EndDate));
            c.HasIndex("employee_id", nameof(EmploymentContract.Status));
            c.HasIndex(nameof(EmploymentContract.ContractDocumentId));
        });
        b.Navigation(x => x.EmploymentContracts).UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
