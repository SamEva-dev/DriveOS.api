using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.ExamsCertification;

public sealed class ExamAttemptTests
{
    [Fact]
    public void Attempt_should_follow_nominal_day_of_exam_lifecycle()
    {
        ExamAttempt attempt = CreateAttempt();
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        attempt.CheckIn(Guid.NewGuid(), "checkin", actor, now).IsSuccess.Should().BeTrue();
        attempt.Status.Should().Be(ExamAttemptStatus.CheckedIn);
        attempt.AttendanceStatus.Should().Be(ExamAttendanceStatus.Present);

        attempt.Start(Guid.NewGuid(), "start", actor, now.AddMinutes(10)).IsSuccess.Should().BeTrue();
        attempt.Status.Should().Be(ExamAttemptStatus.InProgress);

        attempt.Complete(Guid.NewGuid(), "complete", actor, now.AddMinutes(45)).IsSuccess.Should().BeTrue();
        attempt.Status.Should().Be(ExamAttemptStatus.AwaitingResult);
        attempt.CompletedAtUtc.Should().NotBeNull();
        attempt.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void Attempt_cannot_start_before_candidate_check_in()
    {
        ExamAttempt attempt = CreateAttempt();
        UserId actor = new(Guid.NewGuid());

        attempt.Start(Guid.NewGuid(), "start", actor, DateTimeOffset.UtcNow).IsFailure.Should().BeTrue();
        attempt.Status.Should().Be(ExamAttemptStatus.Scheduled);
    }

    [Fact]
    public void Candidate_absence_is_terminal_and_distinguishes_excused_absence()
    {
        ExamAttempt attempt = CreateAttempt();
        UserId actor = new(Guid.NewGuid());

        attempt.MarkAbsent(true, "MedicalEvidence", "Justificatif reçu", Guid.NewGuid(), "absent", actor, DateTimeOffset.UtcNow)
            .IsSuccess.Should().BeTrue();

        attempt.Status.Should().Be(ExamAttemptStatus.CandidateAbsent);
        attempt.AttendanceStatus.Should().Be(ExamAttendanceStatus.ExcusedAbsent);
        attempt.OperationalReasonCode.Should().Be("MedicalEvidence");
    }

    [Fact]
    public void In_progress_attempt_can_be_interrupted_but_not_postponed()
    {
        ExamAttempt attempt = CreateAttempt();
        UserId actor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        attempt.CheckIn(Guid.NewGuid(), "checkin", actor, now);
        attempt.Start(Guid.NewGuid(), "start", actor, now.AddMinutes(1));

        attempt.Postpone("CenterIssue", null, Guid.NewGuid(), "postpone", actor, now.AddMinutes(2)).IsFailure.Should().BeTrue();
        attempt.Interrupt("SafetyIncident", "Épreuve interrompue", Guid.NewGuid(), "interrupt", actor, now.AddMinutes(3)).IsSuccess.Should().BeTrue();
        attempt.Status.Should().Be(ExamAttemptStatus.Interrupted);
        attempt.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Reusing_same_operation_id_with_different_payload_is_rejected()
    {
        ExamAttempt attempt = CreateAttempt();
        UserId actor = new(Guid.NewGuid());
        Guid operationId = Guid.NewGuid();

        attempt.CheckIn(operationId, "same", actor, DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        attempt.CheckIn(operationId, "different", actor, DateTimeOffset.UtcNow.AddMinutes(1)).IsFailure.Should().BeTrue();
    }

    private static ExamAttempt CreateAttempt()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(2);
        return ExamAttempt.Create(
            new OrganizationId(Guid.NewGuid()),
            ExamRegistrationId.New(),
            ExamPreparationId.New(),
            new PersonId(Guid.NewGuid()),
            1,
            3,
            2,
            "Practical",
            "B",
            ExamCenterId.New(),
            ExamPlaceId.New(),
            start,
            start.AddMinutes(45),
            start.AddMinutes(-45),
            new UserId(Guid.NewGuid()),
            new VehicleId(Guid.NewGuid()),
            new BookingId(Guid.NewGuid()),
            Guid.NewGuid(),
            "create",
            new UserId(Guid.NewGuid()),
            DateTimeOffset.UtcNow).Value;
    }
}
