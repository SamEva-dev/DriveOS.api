using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed record CorrectTrainingSessionAttendanceCommand(
    OrganizationId OrganizationId,
    TrainingSessionId SessionId,
    Guid OperationId,
    TrainingSessionAttendanceStatus Status,
    DateTimeOffset? ActualArrivalAtUtc,
    DateTimeOffset? ActualDepartureAtUtc,
    string? Reason,
    Guid? EvidenceDocumentId,
    UserId ActorUserId,
    bool IsOverride,
    string? OverrideReason) : ICommand<TrainingSessionResponse>;
