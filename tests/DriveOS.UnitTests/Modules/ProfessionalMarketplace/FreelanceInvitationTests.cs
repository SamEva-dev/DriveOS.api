using DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;
public sealed class FreelanceInvitationTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Invitation_stores_token_hash_not_raw_token()
    {
        var x=FreelanceInvitation.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,null,null,null,
            "teacher@example.test",null,null,new DateOnly(2026,9,30),new DateOnly(2026,9,1),Actor,Now).Value;
        string raw=new string('A',48);
        x.Send(raw,Now,Actor);
        Assert.NotEqual(raw,x.TokenHash);
        Assert.True(x.TokenMatches(raw));
    }

    [Fact]
    public void Acceptance_requires_authenticated_user()
    {
        var x=FreelanceInvitation.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,null,null,null,
            "teacher@example.test",null,null,new DateOnly(2026,9,30),new DateOnly(2026,9,1),Actor,Now).Value;
        string raw=new string('B',48);x.Send(raw,Now,Actor);
        Assert.True(x.Accept(raw,UserId.Empty,new DateOnly(2026,9,2),Now).IsFailure);
    }

    [Fact]
    public void Email_does_not_bind_identity_automatically()
    {
        var x=FreelanceInvitation.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),null,null,null,null,
            "teacher@example.test",null,null,new DateOnly(2026,9,30),new DateOnly(2026,9,1),Actor,Now).Value;
        Assert.Null(x.InvitedUserId);
        Assert.Null(x.AcceptedByUserId);
    }
}
