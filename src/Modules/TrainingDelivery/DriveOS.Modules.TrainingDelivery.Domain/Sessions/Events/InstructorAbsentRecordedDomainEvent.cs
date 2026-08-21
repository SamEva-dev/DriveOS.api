using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;

public sealed record InstructorAbsentRecordedDomainEvent(
    TrainingSessionId SessionId,
    OrganizationId OrganizationId,
    UserId InstructorId,
    TrainingSessionAttendanceId AttendanceId,
    DateTimeOffset RecordedAtUtc) : DomainEvent;
