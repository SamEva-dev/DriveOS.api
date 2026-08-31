using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class FirstSuccessfulCollaborationTests
{
    private static readonly UserId Actor=new(Guid.NewGuid());
    private static readonly DateTimeOffset Now=new(2026,9,1,8,0,0,TimeSpan.Zero);

    private static ProfessionalEngagement Engagement()
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

        return ProfessionalEngagement.Create(new(Guid.NewGuid()),null,offer,Now,Actor).Value;
    }

    [Fact]
    public void First_paid_invoice_completes_initial_integration_and_establishes_reliable_relationship()
    {
        var engagement=Engagement();
        var invoiceId=new ProfessionalInvoiceId(Guid.NewGuid());
        Guid financeInvoiceId=Guid.NewGuid();

        Assert.True(engagement.CompleteInitialIntegration(
            invoiceId,financeInvoiceId,Guid.NewGuid(),"SEPA",Now).IsSuccess);

        Assert.Equal(invoiceId,engagement.FirstPaidProfessionalInvoiceId);
        Assert.Equal("SEPA",engagement.ConfirmedPaymentMethod);
        Assert.True(engagement.IsReliableRelationship);
        Assert.NotNull(engagement.InitialIntegrationCompletedAtUtc);
    }

    [Fact]
    public void Initial_integration_completion_is_idempotent()
    {
        var engagement=Engagement();
        var firstInvoice=new ProfessionalInvoiceId(Guid.NewGuid());

        engagement.CompleteInitialIntegration(firstInvoice,Guid.NewGuid(),Guid.NewGuid(),"SEPA",Now);
        engagement.CompleteInitialIntegration(new(Guid.NewGuid()),Guid.NewGuid(),Guid.NewGuid(),"CARD",Now.AddDays(1));

        Assert.Equal(firstInvoice,engagement.FirstPaidProfessionalInvoiceId);
        Assert.Equal("SEPA",engagement.ConfirmedPaymentMethod);
    }

    [Fact]
    public void Satisfaction_can_only_be_marked_after_initial_integration_completion()
    {
        var engagement=Engagement();

        Assert.True(engagement.MarkSatisfactionRequested(Now).IsFailure);

        engagement.CompleteInitialIntegration(
            new(Guid.NewGuid()),Guid.NewGuid(),Guid.NewGuid(),"SEPA",Now);

        Assert.True(engagement.MarkSatisfactionRequested(Now).IsSuccess);
        Assert.NotNull(engagement.SatisfactionRequestedAtUtc);
    }
}
