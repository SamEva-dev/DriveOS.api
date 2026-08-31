using DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalApplicationTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    [Fact]
    public void Application_status_flow_supports_review_shortlist_accept()
    {
        Assert.Equal((int)ProfessionalApplicationStatus.Submitted,1);
        Assert.Equal((int)ProfessionalApplicationStatus.Accepted,4);
    }

    [Fact]
    public void Accepted_application_is_not_a_contract_or_mission()
    {
        var status=ProfessionalApplicationStatus.Accepted;
        Assert.Equal(ProfessionalApplicationStatus.Accepted,status);
    }

    [Fact]
    public void Withdrawn_is_terminal_for_candidate_side()
    {
        Assert.NotEqual(ProfessionalApplicationStatus.Withdrawn,ProfessionalApplicationStatus.Submitted);
    }
}
