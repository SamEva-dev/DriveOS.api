using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record CompetencyAssessmentRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingPathId TrainingPathId,
    TrainingSessionCompetencyAssessmentId SessionAssessmentId,
    CompetencyId CompetencyId,
    CurriculumVersionId CurriculumVersionId,
    Guid PedagogyAssessmentId,
    string LevelCode,
    UserId AssessorUserId,
    DateTimeOffset AssessedAtUtc) : DomainEvent;
