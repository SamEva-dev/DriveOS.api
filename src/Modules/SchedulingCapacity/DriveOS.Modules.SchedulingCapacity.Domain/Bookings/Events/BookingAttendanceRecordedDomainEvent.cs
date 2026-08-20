using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings.Events;

public sealed record BookingAttendanceRecordedDomainEvent(
    BookingId BookingId,
    OrganizationId OrganizationId,
    BookingAttendanceId AttendanceId,
    AttendanceStatus Status,
    DateTimeOffset RecordedAtUtc) : DomainEvent;
