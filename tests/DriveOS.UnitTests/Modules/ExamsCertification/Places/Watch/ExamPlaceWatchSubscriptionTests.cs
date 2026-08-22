using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Places.Watch;

public sealed class ExamPlaceWatchSubscriptionTests
{
    [Fact]
    public void Create_ShouldNormalizeCentersAndStartDueImmediately()
    {
        DateTimeOffset now = new(2026, 8, 21, 9, 0, 0, TimeSpan.Zero);
        var organizationId = new OrganizationId(Guid.NewGuid());

        var result = ExamPlaceWatchSubscription.Create(
            ExamPlaceWatchSubscriptionId.New(), organizationId, "rdvpermis", "fr", "06", "B",
            now, now.AddDays(30), 5, ["NICE-2", "NICE-1", "NICE-1"], now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ExamPlaceWatchStatus.Active, result.Value.Status);
        Assert.Equal(now, result.Value.NextCheckAtUtc);
        Assert.Equal(["NICE-1", "NICE-2"], result.Value.GetCenterExternalIds());
    }

    [Fact]
    public void ScanFailure_ShouldApplyBoundedBackoff_AndSuccessShouldResetIt()
    {
        DateTimeOffset now = new(2026, 8, 21, 9, 0, 0, TimeSpan.Zero);
        UserId actor = new(Guid.NewGuid());
        var result = ExamPlaceWatchSubscription.Create(
            ExamPlaceWatchSubscriptionId.New(), new OrganizationId(Guid.NewGuid()), "provider", "FR", null, "B",
            now, now.AddDays(10), 30, null, now);
        Assert.True(result.IsSuccess);

        result.Value.RecordFailedScan(now, "provider.timeout", actor);
        Assert.Equal(1, result.Value.ConsecutiveFailureCount);
        Assert.Equal(now.AddMinutes(1), result.Value.NextCheckAtUtc);

        result.Value.RecordSuccessfulScan(now.AddMinutes(2), false, actor);
        Assert.Equal(0, result.Value.ConsecutiveFailureCount);
        Assert.Equal(now.AddMinutes(32), result.Value.NextCheckAtUtc);
        Assert.Null(result.Value.LastErrorCode);
    }

    [Fact]
    public void ProcessingLease_ShouldPreventConcurrentClaimUntilExpiry()
    {
        DateTimeOffset now = new(2026, 8, 21, 9, 0, 0, TimeSpan.Zero);
        var result = ExamPlaceWatchSubscription.Create(
            ExamPlaceWatchSubscriptionId.New(), new OrganizationId(Guid.NewGuid()), "provider", "FR", null, null,
            now, now.AddDays(10), 5, null, now);
        Assert.True(result.IsSuccess);

        Assert.True(result.Value.TryAcquireProcessingLease(Guid.NewGuid(), now.AddMinutes(5), now));
        Assert.False(result.Value.TryAcquireProcessingLease(Guid.NewGuid(), now.AddMinutes(5), now.AddMinutes(1)));
        Assert.True(result.Value.TryAcquireProcessingLease(Guid.NewGuid(), now.AddMinutes(11), now.AddMinutes(6)));
    }
}
