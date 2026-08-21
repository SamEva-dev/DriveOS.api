using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record RecordTrainingSessionCompetencyAssessmentCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    CompetencyId CompetencyId,
    string LevelCode,
    string? ObservedCriteria,
    string? Context,
    TrainingSessionInterventionId? RelatedInterventionId,
    string? InternalComment,
    string? SharedComment,
    Guid? EvidenceDocumentId,
    DateTimeOffset AssessedAtUtc,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
