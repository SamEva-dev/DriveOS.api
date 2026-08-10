using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Modules.Organizations.Domain.Subscriptions.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.OrganizationSubscriptions;

public sealed class OrganizationSubscriptionTests
{
    [Fact]
    public void Create_WithTrialingStatus_ShouldRequireTrialPeriod()
    {
        SubscriptionPlanCode plan = SubscriptionPlanCode.Create("Starter").Value;
        SubscriptionPeriod period = SubscriptionPeriod.Create(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(1)).Value;

        var result = OrganizationSubscription.Create(
            OrganizationSubscriptionId.New(),
            OrganizationId.New(),
            plan,
            SubscriptionStatus.Trialing,
            SubscriptionBillingCycle.Monthly,
            period);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationSubscriptionErrors.InvalidTrialPeriod);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEventAndInitializeVersion()
    {
        OrganizationSubscription subscription = CreateActiveSubscription();

        subscription.Version.Should().Be(1);
        subscription.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrganizationSubscriptionCreatedDomainEvent>();
    }

    [Fact]
    public void ChangePlan_ShouldReplaceEntitlementsAndLimits()
    {
        OrganizationSubscription subscription = CreateActiveSubscription();
        SubscriptionPlanCode plan = SubscriptionPlanCode.Create("Business").Value;

        var result = subscription.ChangePlan(
            plan,
            ["Fleet.Management", "Analytics.Advanced"],
            new Dictionary<string, long>
            {
                ["Branches.Maximum"] = 10,
                ["ActiveStudents.Maximum"] = 500,
            },
            "Upgrade contractuel",
            UserId.New());

        result.IsSuccess.Should().BeTrue();
        subscription.PlanCode.Should().Be(plan);
        subscription.HasEntitlement("Fleet.Management").Should().BeTrue();
        subscription.GetLimit("Branches.Maximum").Should().Be(10);
        subscription.Version.Should().Be(2);
        subscription.DomainEvents.Should().Contain(
            item => item is OrganizationSubscriptionPlanChangedDomainEvent);
    }

    [Fact]
    public void ChangePlan_WithDuplicateEntitlements_ShouldFail()
    {
        OrganizationSubscription subscription = CreateActiveSubscription();

        var result = subscription.ChangePlan(
            SubscriptionPlanCode.Create("Business").Value,
            ["Fleet.Management", "Fleet.Management"],
            new Dictionary<string, long>(),
            "Upgrade",
            UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationSubscriptionErrors.DuplicateEntitlement);
    }

    [Fact]
    public void CancelledSubscription_ShouldRejectPlanChange()
    {
        OrganizationSubscription subscription = CreateActiveSubscription();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        SubscriptionCancellation cancellation = SubscriptionCancellation.Create(
            now,
            now.AddDays(30),
            "Résiliation demandée",
            UserId.New()).Value;

        subscription.Cancel(cancellation).IsSuccess.Should().BeTrue();

        var result = subscription.ChangePlan(
            SubscriptionPlanCode.Create("Enterprise").Value,
            [],
            new Dictionary<string, long>(),
            "Tentative de modification",
            UserId.New());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(
            OrganizationSubscriptionErrors.CancelledSubscriptionCannotBeChanged);
    }

    private static OrganizationSubscription CreateActiveSubscription()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return OrganizationSubscription.Create(
            OrganizationSubscriptionId.New(),
            OrganizationId.New(),
            SubscriptionPlanCode.Create("Professional").Value,
            SubscriptionStatus.Active,
            SubscriptionBillingCycle.Monthly,
            SubscriptionPeriod.Create(now, now.AddMonths(1)).Value).Value;
    }
}
