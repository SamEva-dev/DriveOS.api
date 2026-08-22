using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Providers;
using DriveOS.SharedKernel.Identifiers;
using Xunit;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Places;

public sealed class ExamPlaceSynchronizationTests
{
    [Fact]
    public void MissingProviderPlace_ShouldExpireAvailablePlace()
    {
        ExamPlace place = CreatePlace();
        bool changed = place.MarkUnavailableFromProvider(DateTimeOffset.UtcNow.AddMinutes(1), UserId.New());
        Assert.True(changed);
        Assert.Equal(ExamPlaceStatus.Expired, place.Status);
    }

    [Fact]
    public void ProviderPlace_ShouldReactivateExpiredPlace()
    {
        ExamPlace place = CreatePlace();
        UserId actor = UserId.New();
        place.MarkUnavailableFromProvider(DateTimeOffset.UtcNow.AddMinutes(1), actor);

        bool changed = place.SynchronizeExternalAvailability(
            place.ExamCenterId, "Practical", "B",
            DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(2).AddMinutes(30),
            "Europe/Paris", DateTimeOffset.UtcNow.AddMinutes(2), actor);

        Assert.True(changed);
        Assert.Equal(ExamPlaceStatus.Available, place.Status);
    }

    [Fact]
    public void ProviderDisappearance_ShouldNotUndoAssignedPlace()
    {
        ExamPlace place = CreatePlace();
        UserId actor = UserId.New();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Guid hold = Guid.NewGuid();
        Assert.True(place.Hold(hold, now.AddMinutes(5), actor, now).IsSuccess);
        Assert.True(place.Assign(hold, PersonId.New(), ExamRegistrationId.New(), actor, now.AddSeconds(1)).IsSuccess);

        bool changed = place.MarkUnavailableFromProvider(now.AddMinutes(2), actor);

        Assert.False(changed);
        Assert.Equal(ExamPlaceStatus.Assigned, place.Status);
    }

    private static ExamPlace CreatePlace()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);
        return ExamPlace.Create(
            ExamPlaceId.New(), OrganizationId.New(), ExamCenterId.New(), "Practical", "B",
            start, start.AddMinutes(30), "Europe/Paris", ExamPlaceSource.ExternalProvider,
            "test-provider", "slot-1", DateTimeOffset.UtcNow).Value;
    }
}
