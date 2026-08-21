using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record RecordTrainingSessionInterventionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionInterventionType Type,
    TrainingSessionInterventionSeverity Severity,
    DateTimeOffset OccurredAtUtc,
    string Context,
    string Reason,
    CompetencyId? RelatedCompetencyId,
    string? Outcome,
    string? InternalComment,
    string? SharedExplanation,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;


public sealed record RecordTrainingSessionMarkerCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionMarkerType Type,
    DateTimeOffset OccurredAtUtc,
    CompetencyId? CompetencyId,
    string ShortNote,
    TrainingSessionMarkerSeverity Severity,
    decimal? Latitude,
    decimal? Longitude,
    bool CreatedOffline,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record RecordTrainingSessionObservationCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionObservationType Type,
    DateTimeOffset ObservedAtUtc,
    string Content,
    bool IsInternal,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record InterruptTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionInterruptionReason Reason,
    string? Description,
    DateTimeOffset InterruptedAtUtc,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record ResumeTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    DateTimeOffset ResumedAtUtc,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record RecordTrainingSessionOdometerCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    decimal OdometerKilometers,
    TrainingSessionOdometerSource Source,
    DateTimeOffset ObservedAtUtc,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record RecordTrainingSessionEnergyCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionEnergyEntryType Type,
    decimal? EnergyLevelPercent,
    decimal? Quantity,
    DateTimeOffset ObservedAtUtc,
    string? Note,
    bool CreatedOffline,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
