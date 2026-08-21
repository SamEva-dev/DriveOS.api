using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record MaterializeTrainingSessionCommand(OrganizationId OrganizationId, BookingId BookingId, UserId? ActorUserId) : ICommand<TrainingSessionId>;
