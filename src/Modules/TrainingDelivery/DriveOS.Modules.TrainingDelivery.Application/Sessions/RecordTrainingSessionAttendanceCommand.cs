using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record RecordTrainingSessionAttendanceCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionAttendanceStatus Status,
    DateTimeOffset? ActualArrivalAtUtc,
    DateTimeOffset? ActualDepartureAtUtc,
    string? Reason,
    Guid? EvidenceDocumentId,
    UserId ActorUserId) : ICommand<TrainingSessionResponse>;
