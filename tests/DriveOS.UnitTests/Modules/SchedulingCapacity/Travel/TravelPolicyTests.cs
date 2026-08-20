using DriveOS.Modules.SchedulingCapacity.Domain.Travel;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.Travel;

public sealed class TravelPolicyTests
{
    [Fact]
    public void Evaluate_ShouldUseActualTimes_WhenAvailable()
    {
        DateTimeOffset plannedEnd = new(2026, 8, 19, 10, 0, 0, TimeSpan.Zero);
        DateTimeOffset actualEnd = plannedEnd.AddMinutes(8);
        DateTimeOffset plannedStart = plannedEnd.AddMinutes(40);
        var route = new TravelRouteEstimate(25, 18.5m, "normal", "test");

        TravelFeasibility result = TravelPolicy.Evaluate(plannedEnd, actualEnd, plannedStart, null, route, 10);

        result.DepartureTimeSource.Should().Be(TravelTimeSource.Actual);
        result.AvailableMinutes.Should().Be(32);
        result.RequiredTotalMinutes.Should().Be(35);
        result.IsFeasible.Should().BeFalse();
        result.MarginMinutes.Should().Be(-3);
    }

    [Fact]
    public void ValidateLocation_ShouldRejectContinuousTracking_ByDefault()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var location = new TravelLocation(
            TravelLocationMode.ContinuousAuthorizedTracking,
            "Instructor",
            null,
            43.7,
            7.26,
            "route coordination",
            now,
            now.AddMinutes(30));

        Action act = () => TravelPolicy.ValidateLocation(location, now, false);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidateLocation_ShouldAllowManualAddress_WithoutGeolocation()
    {
        var location = new TravelLocation(
            TravelLocationMode.ManualAddress,
            "Nice Ouest",
            "Nice Ouest, France",
            null,
            null,
            null,
            null,
            null);

        Action act = () => TravelPolicy.ValidateLocation(location, DateTimeOffset.UtcNow, false);

        act.Should().NotThrow();
    }
}
