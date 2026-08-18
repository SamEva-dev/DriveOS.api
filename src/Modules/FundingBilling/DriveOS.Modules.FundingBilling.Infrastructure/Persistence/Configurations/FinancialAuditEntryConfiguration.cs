using DriveOS.Modules.FundingBilling.Infrastructure.Auditing;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class FinancialAuditEntryConfiguration : IEntityTypeConfiguration<FinancialAuditEntry>
{
    public void Configure(EntityTypeBuilder<FinancialAuditEntry> builder)
    {
        builder.ToTable("financial_audit_entries");
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventId).HasColumnName("event_id").ValueGeneratedNever();
        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(x => x.Value, x => new OrganizationId(x))
            .IsRequired();
        builder.Property(x => x.BillingAccountId)
            .HasColumnName("billing_account_id")
            .HasConversion(x => x.Value, x => new BillingAccountId(x))
            .IsRequired();
        builder.Property(x => x.AggregateType).HasColumnName("aggregate_type").HasMaxLength(80).IsRequired();
        builder.Property(x => x.AggregateId).HasColumnName("aggregate_id").IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(160).IsRequired();
        builder.Property(x => x.ActorUserId)
            .HasColumnName("actor_user_id")
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new UserId(x.Value) : null);
        builder.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(x => x.DetailsJson).HasColumnName("details_json").HasColumnType("jsonb");

        builder.HasIndex(x => new { x.OrganizationId, x.BillingAccountId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.Action, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.OrganizationId, x.AggregateType, x.AggregateId });
    }
}
