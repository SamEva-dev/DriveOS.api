using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record StudentAbsentRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingSessionAttendanceId AttendanceId,
    TrainingSessionAttendanceStatus Status,
    DateTimeOffset RecordedAtUtc) : DomainEvent;
