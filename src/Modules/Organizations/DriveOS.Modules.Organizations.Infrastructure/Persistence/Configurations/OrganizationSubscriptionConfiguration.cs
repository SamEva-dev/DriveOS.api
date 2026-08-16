using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationSubscriptionConfiguration
    : IEntityTypeConfiguration<OrganizationSubscription>
{
    public void Configure(EntityTypeBuilder<OrganizationSubscription> builder)
    {
        builder.ToTable("organization_subscriptions");
        builder.HasKey(subscription => subscription.Id);

        builder
            .Property(subscription => subscription.Id)
            .HasConversion(id => id.Value, value => new OrganizationSubscriptionId(value))
            .ValueGeneratedNever();

        builder
            .Property(subscription => subscription.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder
            .Property(subscription => subscription.PlanCode)
            .HasColumnName("plan_code")
            .HasConversion(code => code.Value, value => SubscriptionPlanCode.Create(value).Value)
            .HasMaxLength(SubscriptionPlanCode.MaximumLength)
            .IsRequired();

        builder
            .Property(subscription => subscription.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(subscription => subscription.BillingCycle)
            .HasColumnName("billing_cycle")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(
            subscription => subscription.CurrentPeriod,
            period =>
            {
                period
                    .Property(value => value.StartsAtUtc)
                    .HasColumnName("current_period_starts_at_utc")
                    .IsRequired();
                period
                    .Property(value => value.EndsAtUtc)
                    .HasColumnName("current_period_ends_at_utc");
            }
        );

        builder.OwnsOne(
            subscription => subscription.TrialPeriod,
            period =>
            {
                period.Property(value => value.StartsAtUtc).HasColumnName("trial_starts_at_utc");
                period.Property(value => value.EndsAtUtc).HasColumnName("trial_ends_at_utc");
            }
        );

        builder.OwnsOne(
            subscription => subscription.Cancellation,
            cancellation =>
            {
                cancellation
                    .Property(value => value.RequestedAtUtc)
                    .HasColumnName("cancellation_requested_at_utc");
                cancellation
                    .Property(value => value.EffectiveAtUtc)
                    .HasColumnName("cancellation_effective_at_utc");
                cancellation
                    .Property(value => value.Reason)
                    .HasColumnName("cancellation_reason")
                    .HasMaxLength(SubscriptionCancellation.ReasonMaximumLength);
                cancellation
                    .Property(value => value.RequestedByUserId)
                    .HasColumnName("cancellation_requested_by_user_id")
                    .HasConversion(id => id.Value, value => new UserId(value));
            }
        );

        builder
            .Property(subscription => subscription.ExternalProvider)
            .HasColumnName("external_provider")
            .HasMaxLength(80);

        builder
            .Property(subscription => subscription.ExternalSubscriptionId)
            .HasColumnName("external_subscription_id")
            .HasMaxLength(160);

        builder
            .Property(subscription => subscription.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(subscription => subscription.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder
            .Property(subscription => subscription.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        builder
            .Property(subscription => subscription.LastModifiedAtUtc)
            .HasColumnName("last_modified_at_utc");

        builder
            .Property(subscription => subscription.LastModifiedByUserId)
            .HasColumnName("last_modified_by_user_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null
            );

        const string subscriptionForeignKey = "OrganizationSubscriptionId";

        builder.OwnsMany(
            subscription => subscription.Entitlements,
            entitlement =>
            {
                entitlement.ToTable("organization_subscription_entitlements");

                entitlement.WithOwner().HasForeignKey(subscriptionForeignKey);

                entitlement
                    .Property<OrganizationSubscriptionId>(subscriptionForeignKey)
                    .HasColumnName("organization_subscription_id")
                    .HasConversion(id => id.Value, value => new OrganizationSubscriptionId(value));

                entitlement
                    .Property(value => value.Code)
                    .HasColumnName("entitlement_code")
                    .HasMaxLength(SubscriptionEntitlement.CodeMaximumLength)
                    .IsRequired();

                entitlement.HasKey(subscriptionForeignKey, nameof(SubscriptionEntitlement.Code));
            }
        );

        builder.OwnsMany(
            subscription => subscription.Limits,
            limit =>
            {
                limit.ToTable("organization_subscription_limits");

                limit.WithOwner().HasForeignKey(subscriptionForeignKey);

                limit
                    .Property<OrganizationSubscriptionId>(subscriptionForeignKey)
                    .HasColumnName("organization_subscription_id")
                    .HasConversion(id => id.Value, value => new OrganizationSubscriptionId(value));

                limit
                    .Property(value => value.Code)
                    .HasColumnName("limit_code")
                    .HasMaxLength(SubscriptionLimit.CodeMaximumLength)
                    .IsRequired();

                limit.Property(value => value.Value).HasColumnName("limit_value").IsRequired();

                limit.HasKey(subscriptionForeignKey, nameof(SubscriptionLimit.Code));
            }
        );

        builder
            .HasOne<Domain.Organizations.Organization>()
            .WithMany()
            .HasForeignKey(subscription => subscription.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(subscription => subscription.OrganizationId)
            .IsUnique()
            .HasDatabaseName("ux_organization_subscriptions_organization_id");

        builder
            .HasIndex(subscription => new
            {
                subscription.ExternalProvider,
                subscription.ExternalSubscriptionId,
            })
            .IsUnique()
            .HasFilter("external_provider IS NOT NULL AND external_subscription_id IS NOT NULL")
            .HasDatabaseName("ux_organization_subscriptions_external_reference");

        builder.Ignore(subscription => subscription.DomainEvents);
    }
}
