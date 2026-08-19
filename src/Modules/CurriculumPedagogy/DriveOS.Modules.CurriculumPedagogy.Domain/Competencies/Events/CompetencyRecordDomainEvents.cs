using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.Competencies.Events;

public sealed record CompetencyRecordCreatedDomainEvent(
    CompetencyRecordId CompetencyRecordId,
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    CurriculumVersionId CurriculumVersionId,
    CompetencyId CompetencyId) : DomainEvent;

public sealed record CompetencyAssessedDomainEvent(
    CompetencyRecordId CompetencyRecordId,
    CompetencyAssessmentId AssessmentId,
    TrainingPathId TrainingPathId,
    CompetencyId CompetencyId,
    string LevelCode,
    UserId AssessorUserId,
    Guid? SourceSessionId,
    DateTimeOffset AssessedAtUtc) : DomainEvent;

public sealed record CompetencyLevelChangedDomainEvent(
    CompetencyRecordId CompetencyRecordId,
    TrainingPathId TrainingPathId,
    CompetencyId CompetencyId,
    string? PreviousLevelCode,
    string CurrentLevelCode,
    DateTimeOffset AssessedAtUtc) : DomainEvent;
