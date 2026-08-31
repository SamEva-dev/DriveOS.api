using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalMissionTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,9,1,8,0,0,TimeSpan.Zero);

    private static ProfessionalEngagement ActiveEngagement()
    {
        var terms=new CommercialOfferTerms(
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),["B"],
            ProfessionalEngagementType.FixedMission,
            ProfessionalVehicleProvisionMode.Either,
            2400,35m,"EUR",ProfessionalRateUnit.Hour,null,null,null,[]);

        var offer=ProfessionalCommercialOffer.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
            new ProfessionalApplicationId(Guid.NewGuid()),null,null,
            terms,Now,Actor).Value;

        offer.Send(Now,Actor);
        offer.AcceptByOrganization(Now,Actor);
        offer.AcceptByProfessional(Now,Actor);
        offer.FinalizeOffer(Now,Actor);

        var engagement=ProfessionalEngagement.Create(
            new(Guid.NewGuid()),null,offer,Now,Actor).Value;

        foreach(var step in Enum.GetValues<EngagementPreparationStep>())
            engagement.MarkPreparation(step,true,Now,Actor);

        engagement.Activate(new DateOnly(2026,9,1),Now,Actor);
        return engagement;
    }

    [Fact]
    public void Mission_must_be_inside_engagement_period()
    {
        var engagement=ActiveEngagement();

        var result=ProfessionalMission.Create(
            new(Guid.NewGuid()),engagement,null,"Mission B",null,
            new DateOnly(2026,8,30),new DateOnly(2026,9,10),["B"],600,
            ProfessionalVehicleProvisionMode.Either,[],Now,Actor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Mission_categories_must_be_covered_by_engagement()
    {
        var result=ProfessionalMission.Create(
            new(Guid.NewGuid()),ActiveEngagement(),null,"Mission A2",null,
            new DateOnly(2026,9,2),new DateOnly(2026,9,10),["A2"],600,
            ProfessionalVehicleProvisionMode.Either,[],Now,Actor);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Mission_requires_professional_acceptance_before_activation()
    {
        var mission=ProfessionalMission.Create(
            new(Guid.NewGuid()),ActiveEngagement(),null,"Mission B",null,
            new DateOnly(2026,9,1),new DateOnly(2026,9,10),["B"],600,
            ProfessionalVehicleProvisionMode.Either,
            [new(DayOfWeek.Tuesday,new(9,0),new(12,0),"Europe/Paris")],
            Now,Actor).Value;

        mission.Propose(Now,Actor);

        Assert.Equal(ProfessionalMissionStatus.Proposed,mission.Status);
        Assert.True(mission.Activate(new DateOnly(2026,9,1),Now,Actor).IsFailure);
    }

    [Fact]
    public void Accepted_mission_can_activate_inside_its_period()
    {
        var mission=ProfessionalMission.Create(
            new(Guid.NewGuid()),ActiveEngagement(),null,"Mission B",null,
            new DateOnly(2026,9,1),new DateOnly(2026,9,10),["B"],600,
            ProfessionalVehicleProvisionMode.Either,[],Now,Actor).Value;

        mission.Propose(Now,Actor);
        mission.Accept(Now,Actor);

        Assert.True(mission.Activate(new DateOnly(2026,9,1),Now,Actor).IsSuccess);
        Assert.Equal(ProfessionalMissionStatus.Active,mission.Status);
    }
}
