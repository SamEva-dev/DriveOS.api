using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalMarketplaceCompliancePolicyTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,8,24,20,0,0,TimeSpan.Zero);

    [Fact]
    public void Requirement_can_be_scoped_to_teaching_category()
    {
        var r=ProfessionalComplianceRequirement.Create(
            new(Guid.NewGuid()),"FR.INSTRUCTOR.B.AUTH","FR",ProfessionalType.DrivingInstructor,
            ProfessionalEvidenceKind.Credential,"TEACHING_AUTHORIZATION",true,true,["B"],
            new DateOnly(2026,1,1),null,1,Now,Actor).Value;

        Assert.True(r.AppliesOn(new DateOnly(2026,8,24),["B"]));
        Assert.False(r.AppliesOn(new DateOnly(2026,8,24),["A2"]));
    }

    [Fact]
    public void Non_verified_profile_cannot_be_made_visible()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        var result=p.ChangeMarketplaceVisibility(MarketplaceVisibility.Public,Now,Actor);
        Assert.True(result.IsFailure);
        Assert.Equal(MarketplaceVisibility.Private,p.MarketplaceVisibility);
    }

    [Fact]
    public void Compliance_loss_removes_marketplace_visibility()
    {
        var p=ProfessionalProfile.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,Now).Value;
        p.MarkCompliance(ProfessionalComplianceStatus.Compliant,Now,Actor);
        // Direct visibility is still protected by profile completeness; compliance invalidation must always be safe.
        p.MarkCompliance(ProfessionalComplianceStatus.NonCompliant,Now.AddDays(1),Actor);
        Assert.Equal(MarketplaceVisibility.Private,p.MarketplaceVisibility);
        Assert.Equal(MarketplaceVerificationBadge.None,p.VerificationBadge);
    }
}
