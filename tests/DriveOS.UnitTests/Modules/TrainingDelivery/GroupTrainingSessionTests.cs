using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Xunit;

namespace DriveOS.UnitTests.Modules.TrainingDelivery;

public sealed class GroupTrainingSessionTests
{
    [Fact]
    public void Materialize_ShouldRejectParticipantsBeyondCapacity()
    {
        var result = GroupTrainingSession.Materialize(Create(capacity: 1, students: [new PersonId(Guid.NewGuid()), new PersonId(Guid.NewGuid())]));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TrainingDelivery.GroupSession.Capacity.Exceeded");
    }

    [Fact]
    public void AddAuthorizedParticipant_ShouldRespectCapacity()
    {
        var session = GroupTrainingSession.Materialize(Create(2, [new PersonId(Guid.NewGuid())])).Value;
        session.AddAuthorizedParticipant(new PersonId(Guid.NewGuid()), Guid.NewGuid()).IsSuccess.Should().BeTrue();
        session.AddAuthorizedParticipant(new PersonId(Guid.NewGuid()), Guid.NewGuid()).Error.Code.Should().Be("TrainingDelivery.GroupSession.Capacity.Exceeded");
    }

    [Fact]
    public void Attendance_ShouldRemainIndividual()
    {
        var a = new PersonId(Guid.NewGuid()); var b = new PersonId(Guid.NewGuid());
        var session = GroupTrainingSession.Materialize(Create(2, [a,b])).Value;
        session.RecordAttendance(a, GroupSessionAttendanceStatus.Present, GroupSessionAttendanceMethod.Manual, DateTimeOffset.UtcNow, null, Guid.NewGuid(), Guid.NewGuid());
        session.Participants.Single(x=>x.StudentId==a).AttendanceStatus.Should().Be(GroupSessionAttendanceStatus.Present);
        session.Participants.Single(x=>x.StudentId==b).AttendanceStatus.Should().Be(GroupSessionAttendanceStatus.Pending);
    }

    [Fact]
    public void OutsideListParticipant_ShouldBeExplicitlyFlagged()
    {
        var session = GroupTrainingSession.Materialize(Create(2, [new PersonId(Guid.NewGuid())])).Value;
        var student = new PersonId(Guid.NewGuid());
        session.AddAuthorizedParticipant(student, Guid.NewGuid());
        session.Participants.Single(x=>x.StudentId==student).AddedOutsideOriginalList.Should().BeTrue();
    }

    private static GroupTrainingSessionMaterialization Create(int capacity, IReadOnlyCollection<PersonId> students) => new(
        new OrganizationId(Guid.NewGuid()), new BookingId(Guid.NewGuid()), "Signalisation", capacity,
        new UserId(Guid.NewGuid()), new BranchId(Guid.NewGuid()), Guid.NewGuid(), "Salle 2",
        DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), "Comprendre la signalisation", students);
}
