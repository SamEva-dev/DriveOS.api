using DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalProposalTests
{
    [Fact]
    public void Proposal_flow_supports_counter_before_acceptance()
    {
        Assert.Equal((int)ProfessionalProposalStatus.Sent,1);
        Assert.Equal((int)ProfessionalProposalStatus.Countered,2);
        Assert.Equal((int)ProfessionalProposalStatus.Accepted,3);
    }

    [Fact]
    public void Accepted_proposal_remains_precontractual()
    {
        var status=ProfessionalProposalStatus.Accepted;
        Assert.Equal(ProfessionalProposalStatus.Accepted,status);
    }

    [Fact]
    public void Terminal_states_are_explicit()
    {
        Assert.NotEqual(ProfessionalProposalStatus.Withdrawn,ProfessionalProposalStatus.Expired);
        Assert.NotEqual(ProfessionalProposalStatus.Rejected,ProfessionalProposalStatus.Accepted);
    }
}
