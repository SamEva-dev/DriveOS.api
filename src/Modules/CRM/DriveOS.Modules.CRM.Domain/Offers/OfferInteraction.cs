using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Offers;

public sealed class OfferInteraction : Entity<OfferInteractionId>
{
    private OfferInteraction() { }

    internal OfferInteraction(
        OfferInteractionId id,
        CommercialOfferId offerId,
        OfferInteractionType type,
        DateTimeOffset occurredAtUtc,
        UserId? actorUserId,
        string? summary,
        string? metadataJson
    )
        : base(id)
    {
        OfferId = offerId;
        Type = type;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        ActorUserId = actorUserId;
        Summary = summary;
        MetadataJson = metadataJson;
    }

    public CommercialOfferId OfferId { get; private set; }
    public OfferInteractionType Type { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public UserId? ActorUserId { get; private set; }
    public string? Summary { get; private set; }
    public string? MetadataJson { get; private set; }
}

public enum OfferInteractionType
{
    Created = 1,
    Sent = 2,
    Viewed = 3,
    QuestionReceived = 4,
    ModificationRequested = 5,
    FollowUpScheduled = 6,
    FollowUpCompleted = 7,
    Accepted = 8,
    Rejected = 9,
    Withdrawn = 10,
    Expired = 11,
    VersionCreated = 12,
}
