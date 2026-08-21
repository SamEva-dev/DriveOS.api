using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record SaveTrainingSessionReportDraftCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    int ExpectedVersion,
    int LastCompletedStep,
    string? Summary,
    string? ObjectivesWorked,
    string? ObjectivesAchieved,
    string? NextObjective,
    string? SharedComment,
    string? InternalNote,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
