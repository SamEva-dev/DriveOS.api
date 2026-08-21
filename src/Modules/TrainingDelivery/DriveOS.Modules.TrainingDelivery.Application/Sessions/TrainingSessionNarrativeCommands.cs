using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record UpdateTrainingSessionSharedCommentCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    int ExpectedVersion,
    string? Content,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record UpdateTrainingSessionInternalNoteCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    int ExpectedVersion,
    string? Content,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;

public sealed record TrainingSessionNarrativeRevisionResponse(
    Guid Id,
    int Kind,
    int ReportVersion,
    string? Content,
    Guid ChangedByUserId,
    DateTimeOffset ChangedAtUtc);

public sealed record TrainingSessionInternalNoteResponse(
    Guid SessionId,
    int ReportVersion,
    string? InternalNote,
    IReadOnlyCollection<TrainingSessionNarrativeRevisionResponse> History);
