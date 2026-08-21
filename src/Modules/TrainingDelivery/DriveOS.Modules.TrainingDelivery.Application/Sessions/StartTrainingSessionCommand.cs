using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record StartTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    DateTimeOffset StartedAtUtc,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
