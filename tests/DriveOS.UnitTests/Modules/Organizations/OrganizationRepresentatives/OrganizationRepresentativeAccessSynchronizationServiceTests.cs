using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.AccessSynchronization;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.OrganizationRepresentatives;

public sealed class OrganizationRepresentativeAccessSynchronizationServiceTests
{
    [Fact]
    public async Task Active_representative_with_user_is_synchronized()
    {
        FakeSynchronizer synchronizer = new();
        var service = new OrganizationRepresentativeAccessSynchronizationService(synchronizer);
        OrganizationRepresentative representative = CreateRepresentative(withUser: true);
        representative.Activate();

        await service.SynchronizeAsync(representative);

        Assert.Equal(1, synchronizer.SynchronizeCalls);
        Assert.Equal(0, synchronizer.RevokeCalls);
    }

    [Fact]
    public async Task Representative_without_user_does_not_call_AuthGate()
    {
        FakeSynchronizer synchronizer = new();
        var service = new OrganizationRepresentativeAccessSynchronizationService(synchronizer);
        OrganizationRepresentative representative = CreateRepresentative(withUser: false);
        representative.Activate();

        await service.SynchronizeAsync(representative);

        Assert.Equal(0, synchronizer.SynchronizeCalls);
        Assert.Equal(0, synchronizer.RevokeCalls);
    }

    private static OrganizationRepresentative CreateRepresentative(bool withUser)
    {
        var scope = RepresentativeAuthorityScope.Create("General representation").Value;
        return OrganizationRepresentative.Create(
            OrganizationRepresentativeId.New(),
            new OrganizationId(Guid.NewGuid()),
            new PersonId(Guid.NewGuid()),
            withUser ? new UserId(Guid.NewGuid()) : null,
            OrganizationRepresentativeType.LegalRepresentative,
            scope,
            false,
            new DateOnly(2026, 1, 1),
            null).Value;
    }

    private sealed class FakeSynchronizer : IOrganizationRepresentativeAccessSynchronizer
    {
        public int SynchronizeCalls { get; private set; }
        public int RevokeCalls { get; private set; }
        public Task SynchronizeAsync(OrganizationRepresentativeAccessSnapshot representative, CancellationToken cancellationToken = default)
        { SynchronizeCalls++; return Task.CompletedTask; }
        public Task RevokeAsync(OrganizationRepresentativeAccessSnapshot representative, string reason, CancellationToken cancellationToken = default)
        { RevokeCalls++; return Task.CompletedTask; }
    }
}
