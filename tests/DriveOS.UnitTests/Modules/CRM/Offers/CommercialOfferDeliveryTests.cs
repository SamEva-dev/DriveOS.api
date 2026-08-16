using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Offers;

public sealed class CommercialOfferDeliveryTests
{
    [Fact]
    public void PrepareDelivery_requires_an_approved_offer()
    {
        CommercialOffer offer = CreateOffer();
        var result = offer.PrepareDelivery(
            OfferDeliveryChannel.Email,
            "[{\"type\":\"Prospect\"}]",
            "Subject",
            "Message",
            "fr",
            "documents/offer.pdf",
            "[]",
            new string('a', 64),
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(CommercialOfferErrors.InvalidTransition.Code);
    }

    [Fact]
    public void PrepareDelivery_persists_only_the_token_hash()
    {
        CommercialOffer offer = CreateOffer();
        offer.SubmitForReview();
        offer.Approve();
        string hash = new('b', 64);

        var result = offer.PrepareDelivery(
            OfferDeliveryChannel.SecureLink,
            "[{\"type\":\"Prospect\"}]",
            "Subject",
            "Message",
            "fr",
            "documents/offer.pdf",
            "[]",
            hash,
            DateTimeOffset.UtcNow.AddHours(24),
            DateTimeOffset.UtcNow
        );

        result.IsSuccess.Should().BeTrue();
        offer.SecureLinkTokenHash.Should().Be(hash);
        offer.DeliveryStatus.Should().Be(OfferDeliveryStatus.Ready);
    }

    private static CommercialOffer CreateOffer() =>
        CommercialOffer
            .Generate(
                CommercialOfferId.New(),
                OrganizationId.New(),
                LeadId.New(),
                AssessmentSessionId.New(),
                1,
                null,
                1,
                "B",
                "EUR",
                DateTimeOffset.UtcNow.AddDays(10),
                DateTimeOffset.UtcNow,
                0,
                null,
                null,
                null,
                [
                    new CommercialOfferLineDraft(
                        OfferLineType.RegistrationFee,
                        null,
                        "Registration",
                        1,
                        "unit",
                        100,
                        0,
                        20,
                        true,
                        OfferPriceSource.StandardCatalog,
                        null
                    ),
                ]
            )
            .Value;
}
