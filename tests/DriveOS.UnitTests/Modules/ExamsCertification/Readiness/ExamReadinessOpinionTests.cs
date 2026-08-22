using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.ExamsCertification.Readiness;

public sealed class ExamReadinessOpinionTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly PersonId StudentId = new(Guid.NewGuid());
    private static readonly TrainingPathId TrainingPathId = new(Guid.NewGuid());
    private static readonly UserId AuthorId = new(Guid.NewGuid());

    [Fact]
    public void FavorableWithReservations_requires_structured_reservations_and_conditions()
    {
        var result = ExamReadinessOpinion.Submit(
            ExamReadinessOpinionId.New(), OrganizationId, StudentId, TrainingPathId, null, 1,
            ExamReadinessOpinionType.FavorableWithReservations,
            ObservedAutonomyLevel.MostlyAutonomous,
            Array.Empty<ExamReadinessReservationCode>(),
            "Examen blanc conseillé", "Après examen blanc", null,
            82m, 10, 10, true, "Ready", Guid.NewGuid(), AuthorId, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Exams.Readiness.Opinion.ReservationsRequired");
    }

    [Fact]
    public void Submission_keeps_a_server_evidence_snapshot_and_previous_version_link()
    {
        ExamReadinessOpinionId previous = ExamReadinessOpinionId.New();
        var result = ExamReadinessOpinion.Submit(
            ExamReadinessOpinionId.New(), OrganizationId, StudentId, TrainingPathId, previous, 2,
            ExamReadinessOpinionType.Favorable,
            ObservedAutonomyLevel.Autonomous,
            Array.Empty<ExamReadinessReservationCode>(),
            null, null, "Avis favorable",
            91.25m, 12, 12, true, "Ready", Guid.NewGuid(), AuthorId, DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreviousOpinionId.Should().Be(previous);
        result.Value.Version.Should().Be(2);
        result.Value.ProgressPercent.Should().Be(91.25m);
        result.Value.RequiredCompetencies.Should().Be(12);
        result.Value.EvaluatedRequiredCompetencies.Should().Be(12);
        result.Value.HasCompletedPedagogicalReview.Should().BeTrue();
    }

    [Fact]
    public void Request_fingerprint_is_stable_for_the_same_client_submission()
    {
        var codes = new[] { ExamReadinessReservationCode.Autonomy, ExamReadinessReservationCode.MockExamRecommended };
        string first = ExamReadinessOpinion.CreateRequestFingerprint(
            ExamReadinessOpinionType.FavorableWithReservations,
            ObservedAutonomyLevel.MostlyAutonomous,
            codes,
            "Réserve",
            "Condition",
            "Commentaire",
            AuthorId);
        string second = ExamReadinessOpinion.CreateRequestFingerprint(
            ExamReadinessOpinionType.FavorableWithReservations,
            ObservedAutonomyLevel.MostlyAutonomous,
            codes.Reverse().ToArray(),
            " Réserve ",
            "Condition",
            "Commentaire",
            AuthorId);

        first.Should().Be(second);
    }
}
