using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.CRM.Domain.Assessments.Events;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Assessments;

public sealed class AssessmentAppointmentTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly LeadId LeadId = new(Guid.NewGuid());

    [Fact]
    public void Schedule_WithValidPeriod_RaisesScheduledEvent()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);

        var result = ScheduleValid(start, " Initiale ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be("Initiale");
        result
            .Value.DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AssessmentAppointmentScheduledDomainEvent>();
    }

    [Fact]
    public void Reschedule_WhenScheduled_ChangesPeriodAndRaisesEvent()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);
        AssessmentAppointment appointment = ScheduleValid(start).Value;

        var result = appointment.Reschedule(start.AddDays(1), start.AddDays(1).AddHours(2));

        result.IsSuccess.Should().BeTrue();
        appointment
            .DomainEvents.Should()
            .Contain(x => x is AssessmentAppointmentRescheduledDomainEvent);
    }

    [Fact]
    public void Cancel_Twice_ReturnsStableConflictError()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);
        AssessmentAppointment appointment = ScheduleValid(start).Value;

        appointment.Cancel(DateTimeOffset.UtcNow);
        var second = appointment.Cancel(DateTimeOffset.UtcNow);

        second.IsFailure.Should().BeTrue();
        second.Error.Code.Should().Be("Crm.Assessments.AlreadyClosed");
    }

    [Fact]
    public void Schedule_RemoteWithoutVideoConference_ReturnsStableValidationError()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);

        var result = AssessmentAppointment.Schedule(
            AssessmentAppointmentId.New(),
            OrganizationId,
            LeadId,
            null,
            start,
            start.AddHours(1),
            AssessmentType.TheoryAssessment,
            AssessmentDeliveryMode.Remote,
            AssessmentLocationKind.MeetingPoint,
            "Nice",
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Crm.Assessments.Location.InvalidRemote");
    }

    [Fact]
    public void Schedule_WithPrice_NormalizesCurrency()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(1);

        var result = AssessmentAppointment.Schedule(
            AssessmentAppointmentId.New(),
            OrganizationId,
            LeadId,
            null,
            start,
            start.AddHours(1),
            AssessmentType.TheoryAssessment,
            AssessmentDeliveryMode.Remote,
            AssessmentLocationKind.VideoConference,
            "https://meet.example",
            null,
            null,
            null,
            null,
            45m,
            " eur ",
            null
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.PriceCurrency.Should().Be("EUR");
        result.Value.PriceAmount.Should().Be(45m);
    }

    private static DriveOS.SharedKernel.Results.Result<AssessmentAppointment> ScheduleValid(
        DateTimeOffset start,
        string? notes = null
    ) =>
        AssessmentAppointment.Schedule(
            AssessmentAppointmentId.New(),
            OrganizationId,
            LeadId,
            null,
            start,
            start.AddHours(1),
            AssessmentType.TheoryAssessment,
            AssessmentDeliveryMode.Remote,
            AssessmentLocationKind.VideoConference,
            "https://meet.example",
            null,
            null,
            null,
            null,
            null,
            null,
            notes
        );
}
