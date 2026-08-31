using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalEngagementTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,9,1,8,0,0,TimeSpan.Zero);

    private static ProfessionalCommercialOffer FinalizedOffer()
    {
        var terms=new CommercialOfferTerms(
            new DateOnly(2026,9,1),new DateOnly(2026,9,30),["B"],
            ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.Either,
            2400,35m,"EUR",ProfessionalRateUnit.Hour,0.45m,null,500m,["STANDARD"]);

        var offer=ProfessionalCommercialOffer.Create(
            new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),
            new ProfessionalApplicationId(Guid.NewGuid()),null,null,terms,Now,Actor).Value;

        offer.Send(Now,Actor);
        offer.AcceptByOrganization(Now,Actor);
        offer.AcceptByProfessional(Now,Actor);
        offer.FinalizeOffer(Now,Actor);
        return offer;
    }

    [Fact]
    public void Engagement_requires_finalized_offer()
    {
        var offer=FinalizedOffer();
        Assert.Equal(ProfessionalCommercialOfferStatus.Finalized,offer.Status);
        var r=ProfessionalEngagement.Create(new(Guid.NewGuid()),null,offer,Now,Actor);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Activation_requires_all_preparation_steps()
    {
        var engagement=ProfessionalEngagement.Create(new(Guid.NewGuid()),null,FinalizedOffer(),Now,Actor).Value;
        var r=engagement.Activate(new DateOnly(2026,9,1),Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Finalized_commercial_terms_are_snapshotted()
    {
        var offer=FinalizedOffer();
        var engagement=ProfessionalEngagement.Create(new(Guid.NewGuid()),null,offer,Now,Actor).Value;
        Assert.Equal(offer.Revision,engagement.CommercialOfferRevision);
        Assert.Equal(offer.Terms.RateAmount,engagement.TermsSnapshot.RateAmount);
    }

    [Fact]
    public void Engagement_can_activate_after_all_preparations()
    {
        var engagement=ProfessionalEngagement.Create(new(Guid.NewGuid()),null,FinalizedOffer(),Now,Actor).Value;
        foreach(var step in Enum.GetValues<EngagementPreparationStep>())
            engagement.MarkPreparation(step,true,Now,Actor);

        Assert.True(engagement.Activate(new DateOnly(2026,9,1),Now,Actor).IsSuccess);
        Assert.Equal(ProfessionalEngagementStatus.Active,engagement.Status);
    }
}
