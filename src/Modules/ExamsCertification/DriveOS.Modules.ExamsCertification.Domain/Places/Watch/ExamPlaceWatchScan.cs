using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Domain.Places.Watch;

public sealed class ExamPlaceWatchScan : Entity<ExamPlaceWatchScanId>
{
    private ExamPlaceWatchScan() { }

    private ExamPlaceWatchScan(ExamPlaceWatchScanId id, ExamPlaceWatchSubscriptionId subscriptionId,
        OrganizationId organizationId, DateTimeOffset startedAtUtc) : base(id)
    {
        SubscriptionId = subscriptionId;
        OrganizationId = organizationId;
        StartedAtUtc = startedAtUtc.ToUniversalTime();
    }

    public ExamPlaceWatchSubscriptionId SubscriptionId { get; private set; }
    public OrganizationId OrganizationId { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public bool IsSuccess { get; private set; }
    public int ExternalSlotsRead { get; private set; }
    public int NewAvailabilitiesDetected { get; private set; }
    public string? ErrorCode { get; private set; }

    public static ExamPlaceWatchScan Start(ExamPlaceWatchScanId id, ExamPlaceWatchSubscriptionId subscriptionId,
        OrganizationId organizationId, DateTimeOffset startedAtUtc) => new(id, subscriptionId, organizationId, startedAtUtc);

    public void Complete(DateTimeOffset completedAtUtc, int externalSlotsRead, int newAvailabilitiesDetected)
    {
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        ExternalSlotsRead = Math.Max(0, externalSlotsRead);
        NewAvailabilitiesDetected = Math.Max(0, newAvailabilitiesDetected);
        IsSuccess = true;
        ErrorCode = null;
    }

    public void Fail(DateTimeOffset completedAtUtc, string errorCode)
    {
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        IsSuccess = false;
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "integration.failed" : errorCode.Trim();
    }
}
