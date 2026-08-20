using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.Availability;

public sealed class AvailabilityPlanTests
{
    [Fact]
    public void Create_Should_CreateDraftPlan()
    {
        var result = AvailabilityPlan.Create(
            AvailabilityPlanId.New(),
            OrganizationId.New(),
            CalendarResourceId.New(),
            new DateOnly(2026, 8, 1),
            null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AvailabilityPlanStatus.Draft);
    }

    [Fact]
    public void AddRecurringRule_Should_RejectOverlapOnSameDay()
    {
        AvailabilityPlan plan = CreatePlan();

        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(12, 0),
            1).IsSuccess.Should().BeTrue();

        var overlap = plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(13, 0),
            1);

        overlap.IsFailure.Should().BeTrue();
        overlap.Error.Should().Be(AvailabilityPlanErrors.RuleOverlap);
    }

    [Fact]
    public void ResolveCapacity_Should_UseRecurringRule_WhenNoExceptionExists()
    {
        AvailabilityPlan plan = CreatePlan();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            2);
        plan.Activate();

        int capacity = plan.ResolveCapacity(
            new DateOnly(2026, 8, 3),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0));

        capacity.Should().Be(2);
    }

    [Fact]
    public void ResolveCapacity_Should_UseUnavailableExceptionBeforeRecurringRule()
    {
        AvailabilityPlan plan = CreatePlan();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            1);
        plan.AddException(
            AvailabilityExceptionId.New(),
            new DateOnly(2026, 8, 3),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            AvailabilityExceptionType.Unavailable,
            null,
            "Formation interne");
        plan.Activate();

        int capacity = plan.ResolveCapacity(
            new DateOnly(2026, 8, 3),
            new TimeOnly(10, 30),
            new TimeOnly(11, 30));

        capacity.Should().Be(0);
    }

    [Fact]
    public void ActivePlan_Should_AcceptFutureException_ButRejectRecurringRuleMutation()
    {
        AvailabilityPlan plan = CreatePlan();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0));
        plan.Activate();

        plan.AddException(
            AvailabilityExceptionId.New(),
            new DateOnly(2026, 8, 4),
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            AvailabilityExceptionType.Unavailable,
            null,
            "Congé").IsSuccess.Should().BeTrue();

        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Tuesday,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0)).IsFailure.Should().BeTrue();
    }


    [Fact]
    public void AddException_ShouldExposeStructuredSource_ForMaintenance()
    {
        AvailabilityPlan plan = CreatePlan();

        AvailabilityExceptionId exceptionId = AvailabilityExceptionId.New();
        plan.AddException(
            exceptionId,
            new DateOnly(2026, 8, 5),
            new TimeOnly(9, 0),
            new TimeOnly(11, 0),
            AvailabilityExceptionType.Maintenance,
            null,
            "Révision périodique").IsSuccess.Should().BeTrue();

        AvailabilityException exception = plan.Exceptions.Single(x => x.Id == exceptionId);
        exception.Source.Should().Be(AvailabilityExceptionSource.Maintenance);
    }

    [Theory]
    [InlineData(AvailabilityExceptionType.Breakdown, AvailabilityExceptionSource.Breakdown)]
    [InlineData(AvailabilityExceptionType.Cleaning, AvailabilityExceptionSource.Cleaning)]
    [InlineData(AvailabilityExceptionType.Inspection, AvailabilityExceptionSource.Inspection)]
    [InlineData(AvailabilityExceptionType.Transfer, AvailabilityExceptionSource.Transfer)]
    [InlineData(AvailabilityExceptionType.Closure, AvailabilityExceptionSource.Closure)]
    [InlineData(AvailabilityExceptionType.Rental, AvailabilityExceptionSource.Rental)]
    [InlineData(AvailabilityExceptionType.PartnerRestriction, AvailabilityExceptionSource.PartnerRestriction)]
    public void AvailabilityExceptionPolicy_ShouldMapUnavailabilitySource(
        AvailabilityExceptionType type,
        AvailabilityExceptionSource expectedSource)
    {
        AvailabilityExceptionPolicy.IsUnavailable(type).Should().BeTrue();
        AvailabilityExceptionPolicy.ResolveSource(type).Should().Be(expectedSource);
    }


    [Fact]
    public void PreferredRule_Should_NotCreateCapacity_ButShouldExposePreferenceScore()
    {
        AvailabilityPlan plan = CreatePlan();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(17, 0),
            new TimeOnly(19, 0),
            1,
            AvailabilityRuleType.Preferred,
            AvailabilityExceptionSource.StudentDeclared,
            800).IsSuccess.Should().BeTrue();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(20, 0),
            1,
            AvailabilityRuleType.Available,
            AvailabilityExceptionSource.StudentDeclared,
            500).IsSuccess.Should().BeTrue();
        plan.Activate();

        DateOnly date = new(2026, 8, 3);
        plan.ResolveCapacity(date, new TimeOnly(17, 30), new TimeOnly(18, 30)).Should().Be(1);
        plan.ResolvePreferenceScore(date, new TimeOnly(17, 30), new TimeOnly(18, 30)).Should().Be(800);
    }

    [Fact]
    public void HigherPriorityUnavailableException_ShouldOverrideExceptionalAvailability()
    {
        AvailabilityPlan plan = CreatePlan();
        plan.AddRecurringRule(
            AvailabilityRuleId.New(),
            DayOfWeek.Monday,
            new TimeOnly(8, 0),
            new TimeOnly(18, 0),
            1);
        plan.AddException(
            AvailabilityExceptionId.New(),
            new DateOnly(2026, 8, 3),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            AvailabilityExceptionType.Available,
            1,
            "Exceptional availability",
            AvailabilityExceptionSource.SelfDeclared,
            500).IsSuccess.Should().BeTrue();
        plan.AddException(
            AvailabilityExceptionId.New(),
            new DateOnly(2026, 8, 3),
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            AvailabilityExceptionType.Unavailable,
            null,
            "Approved leave",
            AvailabilityExceptionSource.Absence,
            900).IsSuccess.Should().BeTrue();
        plan.Activate();

        plan.ResolveCapacity(new DateOnly(2026, 8, 3), new TimeOnly(10, 30), new TimeOnly(11, 30)).Should().Be(0);
    }

    private static AvailabilityPlan CreatePlan() => AvailabilityPlan.Create(
        AvailabilityPlanId.New(),
        OrganizationId.New(),
        CalendarResourceId.New(),
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 12, 31)).Value;
}
