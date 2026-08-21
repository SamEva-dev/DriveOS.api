using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record TrainingSessionAttendanceRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    TrainingSessionAttendanceId AttendanceId,
    TrainingSessionAttendanceStatus Status,
    UserId RecordedByUserId,
    DateTimeOffset RecordedAtUtc) : DomainEvent;
