using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Organizations.OrganizationSubscriptions.Access;

public sealed class OrganizationSubscriptionAccessCheckerTests
{
    private static readonly OrganizationId OrganizationId = OrganizationId.New();

    [Fact]
    public async Task Entitlement_Require_Should_Succeed_When_Included()
    {
        var checker = new OrganizationEntitlementChecker(
            new FakeReadService(CreateResponse(
                SubscriptionStatus.Active,
                ["Fleet.Management"],
                [])));

        var result = await checker.RequireAsync(OrganizationId, "Fleet.Management");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Entitlement_Require_Should_Fail_When_Not_Included()
    {
        var checker = new OrganizationEntitlementChecker(
            new FakeReadService(CreateResponse(SubscriptionStatus.Active, [], [])));

        var result = await checker.RequireAsync(OrganizationId, "Fleet.Management");

        Assert.True(result.IsFailure);
        Assert.Equal(
            "OrganizationSubscriptions.Access.EntitlementNotIncluded",
            result.Error.Code);
    }

    [Fact]
    public async Task Limit_Check_Should_Return_Unlimited_When_Code_Is_Absent()
    {
        var checker = new OrganizationLimitChecker(
            new FakeReadService(CreateResponse(SubscriptionStatus.Active, [], [])));

        var result = await checker.CheckAsync(
            OrganizationId,
            "Vehicles.Maximum",
            500,
            500);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrganizationLimitAvailability.Unlimited, result.Value.Availability);
    }

    [Fact]
    public async Task Limit_Require_Should_Fail_When_Exceeded()
    {
        var checker = new OrganizationLimitChecker(
            new FakeReadService(CreateResponse(
                SubscriptionStatus.Active,
                [],
                [new SubscriptionLimitResponse("Vehicles.Maximum", 5)])));

        var result = await checker.RequireCapacityAsync(
            OrganizationId,
            "Vehicles.Maximum",
            5,
            1);

        Assert.True(result.IsFailure);
        Assert.Equal("OrganizationSubscriptions.Access.LimitExceeded", result.Error.Code);
    }

    [Fact]
    public async Task Checkers_Should_Block_Suspended_Subscription()
    {
        var response = CreateResponse(
            SubscriptionStatus.Suspended,
            ["Fleet.Management"],
            [new SubscriptionLimitResponse("Vehicles.Maximum", 100)]);

        var entitlementChecker = new OrganizationEntitlementChecker(new FakeReadService(response));
        var limitChecker = new OrganizationLimitChecker(new FakeReadService(response));

        var entitlement = await entitlementChecker.RequireAsync(OrganizationId, "Fleet.Management");
        var limit = await limitChecker.RequireCapacityAsync(OrganizationId, "Vehicles.Maximum", 0, 1);

        Assert.Equal("OrganizationSubscriptions.Access.SubscriptionUnavailable", entitlement.Error.Code);
        Assert.Equal("OrganizationSubscriptions.Access.SubscriptionUnavailable", limit.Error.Code);
    }

    private static OrganizationSubscriptionResponse CreateResponse(
        SubscriptionStatus status,
        IReadOnlyCollection<string> entitlements,
        IReadOnlyCollection<SubscriptionLimitResponse> limits) =>
        new(
            Guid.NewGuid(),
            OrganizationId.Value,
            "Professional",
            (int)status,
            1,
            new SubscriptionPeriodResponse(DateTimeOffset.UtcNow, null),
            null,
            null,
            null,
            null,
            entitlements.Select(code => new SubscriptionEntitlementResponse(code)).ToArray(),
            limits,
            1,
            DateTimeOffset.UtcNow,
            null);

    private sealed class FakeReadService(
        OrganizationSubscriptionResponse? response) : IOrganizationSubscriptionReadService
    {
        public Task<OrganizationSubscriptionResponse?> GetByOrganizationIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default) => Task.FromResult(response);

        public Task<bool> HasEntitlementAsync(
            OrganizationId organizationId,
            string entitlementCode,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<long?> GetLimitAsync(
            OrganizationId organizationId,
            string limitCode,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
