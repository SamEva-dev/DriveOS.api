using DriveOS.Modules.CRM.Application.Activities.GetActivities;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.CRM.Activities;

public sealed class GetCrmActivitiesQueryHandlerTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly LeadId LeadId = new(Guid.NewGuid());

    [Fact]
    public async Task PublicScope_ShouldExcludeInternalActivities()
    {
        var repository = new FakeRepository([Create("Public", false), Create("Interne", true)]);
        var handler = new GetLeadActivitiesQueryHandler(repository);

        var result = await handler.Handle(
            new GetLeadActivitiesQuery(OrganizationId, LeadId, CrmActivityReadScope.PublicOnly),
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Public", result.Value[0].Subject);
    }

    [Fact]
    public async Task InternalScope_ShouldIncludeInternalActivities()
    {
        var repository = new FakeRepository([Create("Public", false), Create("Interne", true)]);
        var handler = new GetRecentActivitiesQueryHandler(repository);

        var result = await handler.Handle(
            new GetRecentActivitiesQuery(OrganizationId, 200, CrmActivityReadScope.IncludeInternal),
            CancellationToken.None
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    private static CrmActivity Create(string subject, bool isInternal) =>
        CrmActivity
            .Create(
                CrmActivityId.New(),
                OrganizationId,
                LeadId,
                CrmActivityType.Note,
                CrmActivityDirection.None,
                subject,
                null,
                DateTimeOffset.UtcNow,
                null,
                CrmActivityMetadata.Manual(isInternal: isInternal)
            )
            .Value;

    private sealed class FakeRepository(IReadOnlyList<CrmActivity> activities)
        : ICrmActivityRepository
    {
        public void Add(CrmActivity activity) => throw new NotSupportedException();

        public Task<IReadOnlyList<CrmActivity>> GetByLeadAsync(
            OrganizationId organizationId,
            LeadId leadId,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(activities);

        public Task<IReadOnlyList<CrmActivity>> GetRecentAsync(
            OrganizationId organizationId,
            int limit,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(activities);

        public Task<CrmActivity?> GetByIdempotencyKeyAsync(
            OrganizationId organizationId,
            string idempotencyKey,
            CancellationToken cancellationToken = default
        ) => Task.FromResult<CrmActivity?>(null);
    }
}
