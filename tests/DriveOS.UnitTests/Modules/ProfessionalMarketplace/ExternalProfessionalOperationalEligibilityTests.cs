using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ExternalProfessionalOperationalEligibilityTests
{
    [Fact]
    public void Unknown_user_is_distinct_from_known_but_ineligible_external_professional()
    {
        var unknown=new ExternalProfessionalOperationalEligibility(false,false,"professional-marketplace.profile.not-found",null);
        var known=new ExternalProfessionalOperationalEligibility(true,false,"professional-marketplace.engagement.not-active",null);

        Assert.False(unknown.IsKnownExternalProfessional);
        Assert.True(known.IsKnownExternalProfessional);
        Assert.False(known.IsEligible);
    }

    [Fact]
    public void Eligible_external_professional_carries_engagement_identifier()
    {
        var id=new ProfessionalEngagementId(Guid.NewGuid());
        var result=new ExternalProfessionalOperationalEligibility(true,true,null,id);

        Assert.True(result.IsEligible);
        Assert.Equal(id,result.EngagementId);
    }
}
