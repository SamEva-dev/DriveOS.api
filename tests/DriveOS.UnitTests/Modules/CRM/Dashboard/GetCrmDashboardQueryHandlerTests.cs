using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Dashboard.GetDashboard;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Dashboard;

public sealed class GetCrmDashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_OrganizationScope_ForwardsTenantAndClock()
    {
        var now = new DateTimeOffset(2026, 8, 11, 12, 0, 0, TimeSpan.Zero);
        var service = new FakeDashboardReadService();
        var handler = new GetCrmDashboardQueryHandler(service, new FakeClock(now));
        OrganizationId organizationId = OrganizationId.New();

        var result = await handler.Handle(
            new GetCrmDashboardQuery(
                [organizationId],
                "organization",
                Guid.NewGuid(),
                EmptyFilters()
            ),
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        service.OrganizationIds.Should().Equal(organizationId);
        service.BranchId.Should().BeNull();
        service.NowUtc.Should().Be(now);
        service.Filters.Should().Be(EmptyFilters());
    }

    [Fact]
    public async Task Handle_BranchScope_ForwardsBranchId()
    {
        var service = new FakeDashboardReadService();
        var handler = new GetCrmDashboardQueryHandler(
            service,
            new FakeClock(DateTimeOffset.UtcNow)
        );
        Guid branchId = Guid.NewGuid();

        await handler.Handle(
            new GetCrmDashboardQuery([OrganizationId.New()], "branch", branchId, EmptyFilters()),
            CancellationToken.None
        );

        service.BranchId.Should().Be(branchId);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeDashboardReadService : ICrmDashboardReadService
    {
        public IReadOnlyCollection<OrganizationId> OrganizationIds { get; private set; } = [];
        public Guid? BranchId { get; private set; }
        public DateTimeOffset NowUtc { get; private set; }
        public CrmDashboardFilters? Filters { get; private set; }

        public Task<CrmDashboardResponse> GetAsync(
            IReadOnlyCollection<OrganizationId> organizationIds,
            Guid? branchId,
            CrmDashboardFilters filters,
            DateTimeOffset nowUtc,
            CancellationToken cancellationToken
        )
        {
            OrganizationIds = organizationIds;
            BranchId = branchId;
            NowUtc = nowUtc;
            Filters = filters;
            return Task.FromResult(
                new CrmDashboardResponse(
                    nowUtc,
                    branchId.HasValue ? "Branch" : "Organization",
                    branchId,
                    new CrmDashboardKpis(0, 0, 0, 0, 0, 0, null, null, null, 0, null),
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    [],
                    []
                )
            );
        }
    }

    private static CrmDashboardFilters EmptyFilters() => new(null, null, null, null, null);
}
