using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class BillingPartyConfiguration : IEntityTypeConfiguration<BillingParty>
{
    public void Configure(EntityTypeBuilder<BillingParty> b)
    {
        b.ToTable("billing_parties"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new BillingPartyId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.PersonId).HasColumnName("person_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new PersonId(x.Value) : null);
        b.Property(x => x.PartyOrganizationId).HasColumnName("party_organization_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new OrganizationId(x.Value) : null);
        b.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.MaximumAmount).HasColumnName("maximum_amount").HasPrecision(18, 2);
        b.Property(x => x.EffectiveFrom).HasColumnName("effective_from").IsRequired(); b.Property(x => x.EffectiveTo).HasColumnName("effective_to");
        b.Property(x => x.Priority).HasColumnName("priority").IsRequired(); b.Property(x => x.IsPrimary).HasColumnName("is_primary").IsRequired();
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired(); b.Property(x => x.EndReason).HasColumnName("end_reason").HasMaxLength(1000); b.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null); b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc"); b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Ignore(x => x.CanPay); b.Ignore(x => x.CanFund); b.Ignore(x => x.DomainEvents);
        b.HasIndex(x => new { x.OrganizationId, x.BillingAccountId, x.Status }).HasDatabaseName("ix_billing_parties_org_account_status");
        b.HasIndex(x => new { x.BillingAccountId, x.Priority }).HasDatabaseName("ix_billing_parties_account_priority");
    }
}
