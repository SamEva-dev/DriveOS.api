using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.OrganizationActivationReadiness;

public sealed class OrganizationActivationReadinessServiceTests
{
    [Fact]
    public async Task EvaluateAsync_ShouldBeReady_WhenAllBlockingRulesAreSatisfied()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        IOrganizationActivationReadinessRule[] rules =
        [
            new StubRule(
                10,
                OrganizationActivationRequirementResult.Satisfied("owner", "owner.ok")
            ),
            new StubRule(
                20,
                OrganizationActivationRequirementResult.Satisfied("subscription", "subscription.ok")
            ),
        ];

        var service = new OrganizationActivationReadinessService(rules);

        OrganizationActivationReadinessReport report = await service.EvaluateAsync(organizationId);

        report.IsReady.Should().BeTrue();
        report.BlockingRequirements.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_ShouldNotBeReady_WhenBlockingRuleIsMissing()
    {
        OrganizationId organizationId = new(Guid.NewGuid());
        IOrganizationActivationReadinessRule[] rules =
        [
            new StubRule(
                10,
                OrganizationActivationRequirementResult.Missing(
                    "primary-owner",
                    "primary-owner.missing",
                    OrganizationActivationRequirementSeverity.Blocking
                )
            ),
        ];

        var service = new OrganizationActivationReadinessService(rules);

        OrganizationActivationReadinessReport report = await service.EvaluateAsync(organizationId);

        report.IsReady.Should().BeFalse();
        report.BlockingRequirements.Should().ContainSingle(x => x.Code == "primary-owner");
    }

    [Fact]
    public async Task EvaluateAsync_ShouldRespectRuleOrder()
    {
        List<int> executionOrder = [];
        OrganizationId organizationId = new(Guid.NewGuid());
        IOrganizationActivationReadinessRule[] rules =
        [
            new TrackingRule(20, executionOrder),
            new TrackingRule(10, executionOrder),
        ];

        var service = new OrganizationActivationReadinessService(rules);

        await service.EvaluateAsync(organizationId);

        executionOrder.Should().Equal(10, 20);
    }

    private sealed class StubRule(int order, OrganizationActivationRequirementResult result)
        : IOrganizationActivationReadinessRule
    {
        public int Order => order;

        public Task<OrganizationActivationRequirementResult> EvaluateAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(result);
    }

    private sealed class TrackingRule(int order, ICollection<int> executionOrder)
        : IOrganizationActivationReadinessRule
    {
        public int Order => order;

        public Task<OrganizationActivationRequirementResult> EvaluateAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken = default
        )
        {
            executionOrder.Add(order);
            return Task.FromResult(
                OrganizationActivationRequirementResult.Satisfied(order.ToString(), "ok")
            );
        }
    }
}
