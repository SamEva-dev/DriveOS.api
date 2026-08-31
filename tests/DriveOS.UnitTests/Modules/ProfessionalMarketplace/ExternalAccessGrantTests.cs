using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ExternalAccessGrantTests
{
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Grant_must_stay_inside_engagement_period()
    {
        var result=ExternalAccessGrant.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ExternalAccessResourceType.Student,Guid.NewGuid(),"READ",
            new DateOnly(2026,8,31),new DateOnly(2026,9,10),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Effective_access_requires_active_status_and_date()
    {
        var grant=ExternalAccessGrant.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ExternalAccessResourceType.Mission,Guid.NewGuid(),"READ",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor).Value;

        Assert.True(grant.IsEffectiveOn(new DateOnly(2026,9,15)));
        Assert.False(grant.IsEffectiveOn(new DateOnly(2026,10,1)));
    }

    [Fact]
    public void Revoked_grant_stops_being_effective()
    {
        var grant=ExternalAccessGrant.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,
            ExternalAccessResourceType.Vehicle,Guid.NewGuid(),"READ",
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),Now,Actor).Value;

        Assert.True(grant.Revoke("Mission terminée",Now,Actor).IsSuccess);
        Assert.False(grant.IsEffectiveOn(new DateOnly(2026,9,15)));
    }
}
