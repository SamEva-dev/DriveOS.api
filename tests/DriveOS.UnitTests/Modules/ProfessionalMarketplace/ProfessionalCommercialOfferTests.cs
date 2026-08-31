using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class ProfessionalCommercialOfferTests
{
    private static readonly DateTimeOffset Now=DateTimeOffset.UtcNow;
    private static readonly UserId Actor=new(Guid.NewGuid());

    [Fact]
    public void Finalization_requires_bilateral_acceptance()
    {
        var terms=new CommercialOfferTerms(new DateOnly(2026,9,1),new DateOnly(2026,9,30),["B"],ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.Either,2400,35m,"EUR",ProfessionalRateUnit.Hour,0.45m,null,500m,[]);
        var offer=ProfessionalCommercialOffer.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new ProfessionalApplicationId(Guid.NewGuid()),null,null,terms,Now,Actor).Value;
        offer.Send(Now,Actor);
        offer.AcceptByOrganization(Now,Actor);
        var r=offer.FinalizeOffer(Now,Actor);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void Revising_terms_resets_acceptances_and_increments_revision()
    {
        var terms=new CommercialOfferTerms(new DateOnly(2026,9,1),new DateOnly(2026,9,30),["B"],ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.Either,2400,35m,"EUR",ProfessionalRateUnit.Hour,null,null,null,[]);
        var offer=ProfessionalCommercialOffer.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),null,new ProfessionalProposalId(Guid.NewGuid()),null,terms,Now,Actor).Value;
        offer.Send(Now,Actor);
        offer.AcceptByOrganization(Now,Actor);
        var revised=terms with { RateAmount=38m };
        Assert.True(offer.Revise(revised,Now.AddMinutes(5),Actor).IsSuccess);
        Assert.Equal(2,offer.Revision);
        Assert.Single(offer.RevisionHistory);
        Assert.Equal(1,offer.RevisionHistory[0].Revision);
        Assert.Equal(35m,offer.RevisionHistory[0].Terms.RateAmount);
        Assert.Equal(38m,offer.Terms.RateAmount);
        Assert.Null(offer.OrganizationAcceptedAtUtc);
        Assert.Equal(ProfessionalCommercialOfferStatus.Draft,offer.Status);
    }

    [Fact]
    public void Same_revision_can_be_finalized_after_both_accept()
    {
        var terms=new CommercialOfferTerms(new DateOnly(2026,9,1),new DateOnly(2026,9,30),["B"],ProfessionalEngagementType.FixedMission,ProfessionalVehicleProvisionMode.ClientProvided,1200,32m,"EUR",ProfessionalRateUnit.Hour,null,null,null,["STANDARD_CANCELLATION"]);
        var offer=ProfessionalCommercialOffer.Create(new(Guid.NewGuid()),new(Guid.NewGuid()),new(Guid.NewGuid()),new ProfessionalApplicationId(Guid.NewGuid()),null,null,terms,Now,Actor).Value;
        offer.Send(Now,Actor);
        offer.AcceptByOrganization(Now,Actor);
        offer.AcceptByProfessional(Now,Actor);
        Assert.True(offer.FinalizeOffer(Now,Actor).IsSuccess);
        Assert.Equal(ProfessionalCommercialOfferStatus.Finalized,offer.Status);
    }
}
