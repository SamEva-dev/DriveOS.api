using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Offers;

public sealed class CommercialOfferTrackingTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Views_are_historized_and_update_aggregated_indicators()
    {
        CommercialOffer offer = CreateSentOffer();

        offer.RecordView(Now.AddHours(1)).IsSuccess.Should().BeTrue();
        offer.RecordView(Now.AddHours(2)).IsSuccess.Should().BeTrue();

        offer.Status.Should().Be(CommercialOfferStatus.Viewed);
        offer.ViewCount.Should().Be(2);
        offer.ViewedAtUtc.Should().Be(Now.AddHours(1));
        offer.LastViewedAtUtc.Should().Be(Now.AddHours(2));
        offer.Interactions.Count(x => x.Type == OfferInteractionType.Viewed).Should().Be(2);
    }

    [Fact]
    public void Modification_request_starts_negotiation_without_changing_offer_lines()
    {
        CommercialOffer offer = CreateSentOffer();
        decimal amount = offer.Amount;

        offer
            .RecordExchange(
                OfferInteractionType.ModificationRequested,
                "Different payment schedule",
                null,
                UserId.New(),
                Now.AddHours(1)
            )
            .IsSuccess.Should()
            .BeTrue();

        offer.Status.Should().Be(CommercialOfferStatus.Negotiation);
        offer.Amount.Should().Be(amount);
        offer
            .Interactions.Should()
            .ContainSingle(x => x.Type == OfferInteractionType.ModificationRequested);
    }

    [Fact]
    public void Follow_up_must_be_scheduled_in_the_future()
    {
        CommercialOffer offer = CreateSentOffer();

        offer.ScheduleFollowUp(Now.AddMinutes(-1), null, null, Now).IsFailure.Should().BeTrue();
        offer.ScheduleFollowUp(Now.AddDays(1), "Call lead", null, Now).IsSuccess.Should().BeTrue();
        offer.NextFollowUpAtUtc.Should().Be(Now.AddDays(1));
    }

    private static CommercialOffer CreateSentOffer()
    {
        CommercialOffer offer = CommercialOffer
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
                Now.AddDays(10),
                Now,
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
        offer.SubmitForReview();
        offer.Approve();
        offer.PrepareDelivery(
            OfferDeliveryChannel.SecureLink,
            "[{\"type\":\"Prospect\"}]",
            "Subject",
            "Message",
            "fr",
            "offer.pdf",
            "[]",
            new string('a', 64),
            Now.AddDays(2),
            Now
        );
        offer.MarkSent(Now);
        return offer;
    }
}
