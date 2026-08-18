using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class FundingPlanConfiguration : IEntityTypeConfiguration<FundingPlan>
{
    public void Configure(EntityTypeBuilder<FundingPlan> b)
    {
        b.ToTable("funding_plans"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Id).HasColumnName("id").HasConversion(x=>x.Value,x=>new FundingPlanId(x)).ValueGeneratedNever();
        b.Property(x=>x.OrganizationId).HasColumnName("organization_id").HasConversion(x=>x.Value,x=>new OrganizationId(x)).IsRequired();
        b.Property(x=>x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x=>x.Value,x=>new BillingAccountId(x)).IsRequired();
        b.Property(x=>x.StudentId).HasColumnName("student_id").HasConversion(x=>x.Value,x=>new PersonId(x)).IsRequired();
        b.Property(x=>x.ContractId).HasColumnName("contract_id").IsRequired();
        b.Property(x=>x.TotalCost).HasColumnName("total_cost").HasPrecision(18,2).IsRequired(); b.Property(x=>x.StudentContribution).HasColumnName("student_contribution").HasPrecision(18,2).IsRequired();
        b.Property(x=>x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired(); b.Property(x=>x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x=>x.SubmittedAtUtc).HasColumnName("submitted_at_utc"); b.Property(x=>x.SubmittedByUserId).HasColumnName("submitted_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.ApprovedAtUtc).HasColumnName("approved_at_utc");
        b.Property(x=>x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x=>x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); b.Property(x=>x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null);
        b.Ignore(x=>x.RequestedFundingAmount); b.Ignore(x=>x.ApprovedFundingAmount); b.Ignore(x=>x.PlannedAmount); b.Ignore(x=>x.ApprovedCoverageAmount); b.Ignore(x=>x.RemainingToPlan); b.Ignore(x=>x.RemainingToApprove); b.Ignore(x=>x.DomainEvents);
        b.HasMany(x=>x.Allocations).WithOne().HasForeignKey(x=>x.FundingPlanId).OnDelete(DeleteBehavior.Cascade); b.Navigation(x=>x.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x=>new{x.OrganizationId,x.ContractId}).IsUnique().HasDatabaseName("ux_funding_plans_org_contract"); b.HasIndex(x=>new{x.OrganizationId,x.BillingAccountId,x.Status}).HasDatabaseName("ix_funding_plans_org_account_status");
    }
}

internal sealed class FundingAllocationConfiguration : IEntityTypeConfiguration<FundingAllocation>
{
    public void Configure(EntityTypeBuilder<FundingAllocation> b)
    {
        b.ToTable("funding_allocations"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).HasColumnName("id").HasConversion(x=>x.Value,x=>new FundingAllocationId(x)).ValueGeneratedNever(); b.Property(x=>x.FundingPlanId).HasColumnName("funding_plan_id").HasConversion(x=>x.Value,x=>new FundingPlanId(x)).IsRequired();
        b.Property(x=>x.FinancingPersonId).HasColumnName("financing_person_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new PersonId(x.Value):null); b.Property(x=>x.FinancingOrganizationId).HasColumnName("financing_organization_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new OrganizationId(x.Value):null);
        b.Property(x=>x.RequestedAmount).HasColumnName("requested_amount").HasPrecision(18,2).IsRequired(); b.Property(x=>x.ApprovedAmount).HasColumnName("approved_amount").HasPrecision(18,2).IsRequired(); b.Property(x=>x.ExternalReference).HasColumnName("external_reference").HasMaxLength(250); b.Property(x=>x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x=>x.DecidedAtUtc).HasColumnName("decided_at_utc"); b.Property(x=>x.DecidedByUserId).HasColumnName("decided_by_user_id").HasConversion(x=>x.HasValue?x.Value.Value:(Guid?)null,x=>x.HasValue?new UserId(x.Value):null); b.Property(x=>x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(1000);
        b.HasIndex(x=>x.FundingPlanId).HasDatabaseName("ix_funding_allocations_plan");
    }
}
