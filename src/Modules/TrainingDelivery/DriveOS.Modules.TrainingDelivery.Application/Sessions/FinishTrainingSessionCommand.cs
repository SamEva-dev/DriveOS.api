using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record FinishTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    DateTimeOffset ActualEndAtUtc,
    decimal? EndEnergyLevelPercent,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
