using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Configurations;

internal sealed class TrainingCreditAccountConfiguration : IEntityTypeConfiguration<TrainingCreditAccount>
{
    public void Configure(EntityTypeBuilder<TrainingCreditAccount> b)
    {
        b.ToTable("training_credit_accounts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new TrainingCreditAccountId(x)).ValueGeneratedNever();
        b.Property(x => x.OrganizationId).HasColumnName("organization_id").HasConversion(x => x.Value, x => new OrganizationId(x)).IsRequired();
        b.Property(x => x.BillingAccountId).HasColumnName("billing_account_id").HasConversion(x => x.Value, x => new BillingAccountId(x)).IsRequired();
        b.Property(x => x.CreditType).HasColumnName("credit_type").HasMaxLength(80).IsRequired();
        b.Property(x => x.QuantityPurchased).HasColumnName("quantity_purchased").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.QuantityReserved).HasColumnName("quantity_reserved").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.QuantityConsumed).HasColumnName("quantity_consumed").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.Adjustments).HasColumnName("adjustments").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.ExpirationDate).HasColumnName("expiration_date");
        b.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        b.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        b.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id").HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new UserId(x.Value) : null);
        b.Ignore(x => x.QuantityAvailable);
        b.Ignore(x => x.DomainEvents);
        b.HasMany(x => x.Movements).WithOne().HasForeignKey(x => x.TrainingCreditAccountId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.OrganizationId, x.BillingAccountId }).HasDatabaseName("ix_training_credit_accounts_org_billing_account");
        b.HasIndex(x => new { x.BillingAccountId, x.CreditType, x.ExpirationDate }).IsUnique().HasDatabaseName("ux_training_credit_accounts_account_type_expiration");
    }
}

internal sealed class TrainingCreditMovementConfiguration : IEntityTypeConfiguration<TrainingCreditMovement>
{
    public void Configure(EntityTypeBuilder<TrainingCreditMovement> b)
    {
        b.ToTable("training_credit_movements");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").HasConversion(x => x.Value, x => new TrainingCreditMovementId(x)).ValueGeneratedNever();
        b.Property(x => x.TrainingCreditAccountId).HasColumnName("training_credit_account_id").HasConversion(x => x.Value, x => new TrainingCreditAccountId(x)).IsRequired();
        b.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(200).IsRequired();
        b.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(1000);
        b.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        b.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasConversion(x => x.Value, x => new UserId(x)).IsRequired();
        b.HasIndex(x => new { x.TrainingCreditAccountId, x.Reference }).IsUnique().HasDatabaseName("ux_training_credit_movements_account_reference");
        b.HasIndex(x => new { x.TrainingCreditAccountId, x.OccurredAtUtc }).HasDatabaseName("ix_training_credit_movements_account_date");
    }
}
