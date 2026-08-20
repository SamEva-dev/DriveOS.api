using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity;

public sealed class RecurrenceSeriesTests
{
    [Fact]
    public void GenerateOccurrences_IsIdempotent_AndKeepsExceptions()
    {
        var created = RecurrenceSeries.Create(RecurrenceSeriesId.New(), OrganizationId.New(), null, RecurrenceTargetType.Booking,
            RecurrenceFrequency.Weekly, 1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), null,
            [DayOfWeek.Tuesday], new TimeOnly(14, 0), 90, "Europe/Paris", "Cours collectif", ResourceSelectionPolicy.BestAvailableResources);
        created.IsSuccess.Should().BeTrue();
        RecurrenceSeries series = created.Value;

        series.GenerateOccurrences().Value.Should().Be(5);
        series.GenerateOccurrences().Value.Should().Be(0);
        RecurrenceOccurrence occurrence = series.Occurrences.OrderBy(x => x.ScheduledDate).First();
        series.CancelOccurrence(occurrence.Id, "Jour férié").IsSuccess.Should().BeTrue();
        series.GenerateOccurrences().Value.Should().Be(0);
        occurrence.Status.Should().Be(RecurrenceOccurrenceStatus.Cancelled);
    }

    [Fact]
    public void ChangeFutureRule_SupersedesFutureOccurrences_WithoutDeletingHistory()
    {
        var created = RecurrenceSeries.Create(RecurrenceSeriesId.New(), OrganizationId.New(), null, RecurrenceTargetType.Booking,
            RecurrenceFrequency.Weekly, 1, new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 31), null,
            [DayOfWeek.Tuesday], new TimeOnly(14, 0), 90, "Europe/Paris", "Cours", ResourceSelectionPolicy.BestAvailableResources);
        RecurrenceSeries series = created.Value;
        series.GenerateOccurrences();
        int oldCount = series.Occurrences.Count;

        series.ChangeFutureRule(new DateOnly(2026, 10, 1), RecurrenceFrequency.Weekly, 1, new DateOnly(2026, 10, 31), null,
            [DayOfWeek.Thursday], new TimeOnly(16, 0), 60).IsSuccess.Should().BeTrue();

        series.Occurrences.Count.Should().BeGreaterThan(oldCount);
        series.Occurrences.Should().Contain(x => x.Status == RecurrenceOccurrenceStatus.Superseded);
        series.Revision.Should().Be(2);
    }
}
