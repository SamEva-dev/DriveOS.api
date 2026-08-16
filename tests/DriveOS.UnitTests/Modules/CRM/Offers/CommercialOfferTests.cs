using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Offers;

public sealed class CommercialOfferTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Generate_calculates_catalog_discount_tax_funding_and_remaining_amount()
    {
        var result = CreateOffer(
            [
                new(
                    OfferLineType.PracticalLesson,
                    null,
                    "20 heures",
                    20,
                    "hour",
                    50,
                    100,
                    20,
                    true,
                    OfferPriceSource.StandardCatalog,
                    null
                ),
            ],
            800
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.CatalogAmount.Should().Be(1000);
        result.Value.DiscountAmount.Should().Be(100);
        result.Value.TaxAmount.Should().Be(180);
        result.Value.Amount.Should().Be(1080);
        result.Value.ProspectRemainingAmount.Should().Be(280);
    }

    [Fact]
    public void Manual_override_requires_a_traceable_reason()
    {
        var result = CreateOffer(
            [
                new(
                    OfferLineType.Other,
                    null,
                    "Prix négocié",
                    1,
                    "package",
                    900,
                    0,
                    0,
                    true,
                    OfferPriceSource.ManualOverride,
                    null
                ),
            ],
            0
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Offers.ManualOverride.ReasonRequired");
    }

    [Fact]
    public void Offer_must_be_approved_before_delivery()
    {
        CommercialOffer offer = CreateOffer(
            [
                new(
                    OfferLineType.TheoryTraining,
                    null,
                    "Code en ligne",
                    1,
                    "package",
                    200,
                    0,
                    20,
                    true,
                    OfferPriceSource.StandardCatalog,
                    null
                ),
            ],
            0
        ).Value;

        offer
            .PrepareDelivery(
                OfferDeliveryChannel.Email,
                "[{\"type\":\"Prospect\"}]",
                "Subject",
                "Message",
                "fr",
                "offer.pdf",
                "[]",
                new string('a', 64),
                Now.AddHours(24),
                Now
            )
            .IsFailure.Should()
            .BeTrue();
        offer.SubmitForReview().IsSuccess.Should().BeTrue();
        offer.Approve().IsSuccess.Should().BeTrue();
        offer
            .PrepareDelivery(
                OfferDeliveryChannel.Email,
                "[{\"type\":\"Prospect\"}]",
                "Subject",
                "Message",
                "fr",
                "offer.pdf",
                "[]",
                new string('a', 64),
                Now.AddHours(24),
                Now
            )
            .IsSuccess.Should()
            .BeTrue();
        offer.MarkSent(Now).IsSuccess.Should().BeTrue();
    }

    private static DriveOS.SharedKernel.Results.Result<CommercialOffer> CreateOffer(
        IReadOnlyCollection<CommercialOfferLineDraft> lines,
        decimal funding
    ) =>
        CommercialOffer.Generate(
            CommercialOfferId.New(),
            new OrganizationId(Guid.NewGuid()),
            new LeadId(Guid.NewGuid()),
            AssessmentSessionId.New(),
            4,
            null,
            1,
            "FR-PERMIS-B",
            "eur",
            Now.AddDays(15),
            Now,
            funding,
            "Estimated only",
            "Valid 15 days",
            null,
            lines
        );
}
