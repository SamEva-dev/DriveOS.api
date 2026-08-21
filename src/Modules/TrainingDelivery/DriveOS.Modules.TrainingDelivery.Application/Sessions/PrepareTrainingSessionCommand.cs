using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record PrepareTrainingSessionCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    UserId ActorUserId) : ICommand<TrainingSessionPreparationResponse>;
