using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionAttendanceCorrectedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionAttendanceId PreviousAttendanceId,
    TrainingSessionAttendanceId AttendanceId,
    UserId CorrectedByUserId,
    DateTimeOffset CorrectedAtUtc,
    bool IsOverride) : DomainEvent;
