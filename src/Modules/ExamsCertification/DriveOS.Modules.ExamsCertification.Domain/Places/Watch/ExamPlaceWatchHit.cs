using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch;

public sealed class ExamPlaceWatchHit : Entity<ExamPlaceWatchHitId>
{
    private ExamPlaceWatchHit() { }

    private ExamPlaceWatchHit(ExamPlaceWatchHitId id, ExamPlaceWatchSubscriptionId subscriptionId,
        OrganizationId organizationId, ExamPlaceId examPlaceId, DateTimeOffset detectedAtUtc) : base(id)
    {
        SubscriptionId = subscriptionId;
        OrganizationId = organizationId;
        ExamPlaceId = examPlaceId;
        FirstDetectedAtUtc = detectedAtUtc.ToUniversalTime();
    }

    public ExamPlaceWatchSubscriptionId SubscriptionId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public ExamPlaceId ExamPlaceId { get; private set; }
    public DateTimeOffset FirstDetectedAtUtc { get; private set; }

    public static ExamPlaceWatchHit Create(ExamPlaceWatchHitId id, ExamPlaceWatchSubscriptionId subscriptionId,
        OrganizationId organizationId, ExamPlaceId examPlaceId, DateTimeOffset detectedAtUtc) =>
        new(id, subscriptionId, organizationId, examPlaceId, detectedAtUtc);
}
