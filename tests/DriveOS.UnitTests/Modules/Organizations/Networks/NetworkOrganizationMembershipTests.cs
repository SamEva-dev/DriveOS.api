using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.Organizations.Networks;

public sealed class NetworkOrganizationMembershipTests
{
    [Fact]
    public void Create_WithDifferentOrganizations_CreatesActiveMembership()
    {
        var result = NetworkOrganizationMembership.Create(NetworkOrganizationMembershipId.New(),
            OrganizationId.New(), OrganizationId.New(), DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithSameOrganization_FailsWithStableKey()
    {
        OrganizationId id = OrganizationId.New();
        var result = NetworkOrganizationMembership.Create(NetworkOrganizationMembershipId.New(), id, id,
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Organizations.NetworkMembership.SelfMembership");
    }

    [Fact]
    public void End_AlreadyEnded_Fails()
    {
        var membership = NetworkOrganizationMembership.Create(NetworkOrganizationMembershipId.New(),
            OrganizationId.New(), OrganizationId.New(), DateTimeOffset.UtcNow.AddDays(-1)).Value;
        membership.End(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();

        membership.End(DateTimeOffset.UtcNow).Error.Code.Should()
            .Be("Organizations.NetworkMembership.AlreadyEnded");
    }
}
