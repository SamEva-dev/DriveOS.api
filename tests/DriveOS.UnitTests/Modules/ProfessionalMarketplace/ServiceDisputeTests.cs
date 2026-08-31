using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ServiceDisputeTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;

    private static ServiceDispute Open()=>ServiceDispute.Open(
        new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
        new(Guid.NewGuid()),Guid.NewGuid(),ServiceDisputeReason.Duration,
        "Durée facturée différente de la durée réalisée.",[],Now,Actor).Value;

    [Fact]
    public void First_message_moves_open_dispute_to_discussion()
    {
        var dispute=Open();
        Assert.True(dispute.AddMessage(ServiceDisputeParty.Freelance,"Je joins la feuille de présence.",Now,Actor).IsSuccess);
        Assert.Equal(ServiceDisputeStatus.UnderDiscussion,dispute.Status);
    }

    [Fact]
    public void Dispute_can_wait_for_either_party()
    {
        var dispute=Open();
        Assert.True(dispute.WaitFor(ServiceDisputeParty.Freelance,Now,Actor).IsSuccess);
        Assert.Equal(ServiceDisputeStatus.WaitingForFreelance,dispute.Status);
        Assert.True(dispute.WaitFor(ServiceDisputeParty.School,Now,Actor).IsSuccess);
        Assert.Equal(ServiceDisputeStatus.WaitingForSchool,dispute.Status);
    }

    [Fact]
    public void Resolution_closes_dossier()
    {
        var dispute=Open();
        Assert.True(dispute.Resolve(ServiceDisputeResolutionOutcome.ApproveServiceEntry,"Durée confirmée par les preuves.",Now,Actor).IsSuccess);
        Assert.Equal(ServiceDisputeStatus.Resolved,dispute.Status);
        Assert.True(dispute.AddMessage(ServiceDisputeParty.School,"late",Now,Actor).IsFailure);
    }

    [Fact]
    public void Escalated_dispute_remains_blocking_and_not_closed()
    {
        var dispute=Open();
        Assert.True(dispute.Escalate("Arbitrage responsable requis.",Now,Actor).IsSuccess);
        Assert.Equal(ServiceDisputeStatus.Escalated,dispute.Status);
        Assert.False(dispute.IsClosed);
    }
}
