using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record CompleteTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    DateTimeOffset ActualEndAtUtc,
    string Summary,
    string? ObjectivesWorked,
    string? ObjectivesAchieved,
    string? NextObjective,
    string? InstructorComments,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
