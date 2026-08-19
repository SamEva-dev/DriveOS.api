using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies.Events;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.Competencies;

public sealed class CompetencyRecordTests
{
    private static CompetencyRecord CreateRecord() => CompetencyRecord.Create(
        CompetencyRecordId.New(),
        new OrganizationId(Guid.NewGuid()),
        TrainingPathId.New(),
        CurriculumVersionId.New(),
        CompetencyId.New(),
        true).Value;

    [Fact]
    public void Create_KeepsCurriculumVersionAndStartsWithoutCurrentLevel()
    {
        CurriculumVersionId versionId = CurriculumVersionId.New();
        CompetencyRecord record = CompetencyRecord.Create(
            CompetencyRecordId.New(),
            new OrganizationId(Guid.NewGuid()),
            TrainingPathId.New(),
            versionId,
            CompetencyId.New(),
            true).Value;

        record.CurriculumVersionId.Should().Be(versionId);
        record.CurrentLevelCode.Should().BeNull();
        record.Assessments.Should().BeEmpty();
        record.DomainEvents.Should().ContainSingle(x => x is CompetencyRecordCreatedDomainEvent);
    }

    [Fact]
    public void RecordAssessment_DerivesCurrentLevelFromLatestAssessment()
    {
        CompetencyRecord record = CreateRecord();
        UserId assessor = new(Guid.NewGuid());
        DateTimeOffset first = new(2026, 9, 10, 10, 0, 0, TimeSpan.Zero);
        DateTimeOffset second = first.AddDays(3);

        record.RecordAssessment(CompetencyAssessmentId.New(), "introduced", assessor, null, "Première approche", true, first, first).IsSuccess.Should().BeTrue();
        record.RecordAssessment(CompetencyAssessmentId.New(), "in_progress", assessor, null, "Progression", true, second, second).IsSuccess.Should().BeTrue();

        record.CurrentLevelCode.Should().Be("IN_PROGRESS");
        record.LastAssessedAtUtc.Should().Be(second);
        record.Assessments.Should().HaveCount(2);
    }

    [Fact]
    public void BackfilledOlderAssessment_DoesNotOverwriteCurrentLevel()
    {
        CompetencyRecord record = CreateRecord();
        UserId assessor = new(Guid.NewGuid());
        DateTimeOffset recent = new(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);

        record.RecordAssessment(CompetencyAssessmentId.New(), "ACQUIRED", assessor, null, null, true, recent, recent);
        int levelChangedCount = record.DomainEvents.Count(x => x is CompetencyLevelChangedDomainEvent);

        record.RecordAssessment(
            CompetencyAssessmentId.New(),
            "INTRODUCED",
            assessor,
            null,
            "Import historique",
            false,
            recent.AddDays(-10),
            recent.AddMinutes(1));

        record.CurrentLevelCode.Should().Be("ACQUIRED");
        record.DomainEvents.Count(x => x is CompetencyLevelChangedDomainEvent).Should().Be(levelChangedCount);
    }

    [Fact]
    public void RecordAssessment_WhenLatestLevelChanges_RaisesLevelChangedEvent()
    {
        CompetencyRecord record = CreateRecord();
        UserId assessor = new(Guid.NewGuid());
        DateTimeOffset now = DateTimeOffset.UtcNow;

        record.RecordAssessment(CompetencyAssessmentId.New(), "IN_PROGRESS", assessor, null, null, false, now, now);
        record.RecordAssessment(CompetencyAssessmentId.New(), "NEEDS_REASSESSMENT", assessor, null, "Régression observée", false, now.AddDays(1), now.AddDays(1));

        CompetencyLevelChangedDomainEvent change = record.DomainEvents
            .OfType<CompetencyLevelChangedDomainEvent>()
            .Last();
        change.PreviousLevelCode.Should().Be("IN_PROGRESS");
        change.CurrentLevelCode.Should().Be("NEEDS_REASSESSMENT");
    }

    [Fact]
    public void RecordAssessment_PreservesAuthorDateSessionAndVisibility()
    {
        CompetencyRecord record = CreateRecord();
        UserId assessor = new(Guid.NewGuid());
        Guid sessionId = Guid.NewGuid();
        DateTimeOffset assessedAt = DateTimeOffset.UtcNow.AddHours(-1);
        DateTimeOffset recordedAt = DateTimeOffset.UtcNow;

        CompetencyAssessment assessment = record.RecordAssessment(
            CompetencyAssessmentId.New(),
            "MOSTLY_ACQUIRED",
            assessor,
            sessionId,
            "Bon niveau général",
            true,
            assessedAt,
            recordedAt).Value;

        assessment.AssessorUserId.Should().Be(assessor);
        assessment.SourceSessionId.Should().Be(sessionId);
        assessment.AssessedAtUtc.Should().Be(assessedAt.ToUniversalTime());
        assessment.IsVisibleToStudent.Should().BeTrue();
    }
}
